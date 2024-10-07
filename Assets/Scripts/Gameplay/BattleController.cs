using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JetBrains.Annotations;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BattleController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private AudioClip _bgMusic;
    [SerializeField] private GameObject _king;
    [SerializeField] private Transform _kingAttackPos;
    [SerializeField] private List<AudioClip> _placeAudio;
    [SerializeField] private AudioClip _winSfx;
    [SerializeField] private List<AudioClip> _kingHitSfx;

    private BattleUI _battleUi;
    private GameField _gameField;
    private CharactersDatabase _charactersDatabase;
    private CinemachineVirtualCamera _cinemachineVirtualCamera;
    private CinemachineTrackedDolly _cameraDollyCart;
    private SoundSystem _soundSystem;
    private GameplayPersistentData _gameplayPersistentData;
    private GameSettings _gameSettings;
    private UpgradeUI _upgradeUi;
    private FloatingTextFactory _floatingTextFactory;
    private readonly Dictionary<CharacterPosition, Character> _playerTeamLayout = new ();
    private readonly Dictionary<CharacterPosition, Character> _enemyTeamLayout = new ();
    private BattlePhase _currentBattlePhase = BattlePhase.Deploy;
    private int _currentWaveIndex = -1;
    public int CurrentCameraWaypoint { get; private set; }
    public int ManaPoints { get; private set; }

    public BattlePhase CurrentBattlePhase => _currentBattlePhase;
    public int CoreHp { get; private set; }
    private WavesConfig WavesConfig { get; set; }

    public enum BattlePhase
    {
        Deploy,
        Fight,
        EndGame
    }
    
    public const float STANDARD_DELAY = 0.5f;

    [Inject]
    private void Inject(
        BattleUI battleUI, 
        CharactersDatabase charactersDatabase,
        GameField gameField,
        CinemachineVirtualCamera virtualCamera,
        SoundSystem soundSystem,
        GameSettings gameSettings,
        GameplayPersistentData gameplayPersistentData,
        UpgradeUI upgradeUI,
        FloatingTextFactory floatingTextFactory)
    {
        _battleUi = battleUI;
        _charactersDatabase = charactersDatabase;
        _gameField = gameField;
        _cinemachineVirtualCamera = virtualCamera;
        _cameraDollyCart = _cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        _soundSystem = soundSystem;
        _gameplayPersistentData = gameplayPersistentData;
        _gameSettings = gameSettings;
        _upgradeUi = upgradeUI;
        _floatingTextFactory = floatingTextFactory;
    }

    private void Start()
    {
        _soundSystem.PlayMusicClip(_bgMusic);

        InitializeTeamLayouts();
        
        UpdateCameraPosition(0);
        
        CoreHp = _gameSettings.CoreBaseHp;

        _battleUi.onDeployFinishedClickedAction += () => { UpdateBattlePhase(BattlePhase.Fight); };
        
        InitializeWithData();

        BattleRoutine().Forget();
    }

    private void InitializeTeamLayouts()
    {
        for (var floorIndex = 0; floorIndex < _gameField.Floors.Length; floorIndex++)
        {
            for (var slotIndex = 0; slotIndex < Floor.FLOOR_SLOTS_COUNT; slotIndex++)
            {
                var positionLeft = new CharacterPosition(floorIndex, slotIndex, ZoneType.Left);
                var positionRight = new CharacterPosition(floorIndex, slotIndex, ZoneType.Right);
                _playerTeamLayout.Add(positionLeft, null);
                _enemyTeamLayout.Add(positionRight, null);
            }
        }
    }

    public void UpdateCameraPosition(int waypointIndex)
    {
        CurrentCameraWaypoint = waypointIndex;
        _cameraDollyCart.m_PathPosition = CurrentCameraWaypoint;
    }

    public List<CharacterPosition> GetFreePlayerZones()
    {
        return _playerTeamLayout.Where(pair => pair.Value == null).Select(pair => pair.Key).ToList();
    }
    
    public List<CharacterPosition> GetOccupiedPlayerZones()
    {
        return _playerTeamLayout.Where(pair => pair.Value != null).Select(pair => pair.Key).ToList();
    }
    
    public List<CharacterPosition> GetOccupiedEnemyZones()
    {
        return _enemyTeamLayout.Where(pair => pair.Value != null).Select(pair => pair.Key).ToList();
    }

    private async UniTaskVoid BattleRoutine()
    {
        _currentWaveIndex = 0;
        var wave = WavesConfig.Waves[_currentWaveIndex];
        await SpawnWave(wave);
        
        while (!IsGameWon() && !IsGameLost())
        {
            ManaPoints = _gameSettings.BaseManaPoints;
            
            await UniTask.WaitUntil(ShouldProceedToFightPhase, PlayerLoopTiming.Update, destroyCancellationToken);
            
            await FightPhaseRoutine();

            UpdateBattlePhase(BattlePhase.Deploy);
        }
        
        UpdateBattlePhase(BattlePhase.EndGame);

        OnEndBattle().Forget();
    }

    private void UpdateBattlePhase(BattlePhase phase)
    {
        _currentBattlePhase = phase;
    }

    private bool IsGameLost()
    {
        return CoreHp <= 0;
    }

    private bool IsGameWon()
    {
        return _currentWaveIndex >= WavesConfig.Waves.Count && GetAllAliveEnemies().Count == 0;
    }

    private async UniTaskVoid OnEndBattle()
    {
        bool isWon = !IsGameLost();
        if (isWon)
        {
            _soundSystem.PlayOneShot(_winSfx);
            if (_gameplayPersistentData.currentStage == GameSettings.MAX_STAGE_INDEX)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY) * 2, DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
                SceneManager.LoadScene(5);
            }
            else
            {
                _gameplayPersistentData.currentStage++;
                var upgrades = _gameSettings.GetRandomUpgradePackages(3);
                _upgradeUi.Initialize(upgrades);
                _upgradeUi.Show();
            }
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY) * 2, DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
            SceneManager.LoadScene(4);
        }
    }

    private async UniTask SpawnWave(WaveConfig wave)
    {
        int slotIndex = 0;
        foreach (var characterType in wave.Wave)
        {
            if (slotIndex >= Floor.FLOOR_SLOTS_COUNT)
            {
                Debug.LogError("Floor slot index is out of range during wave spawn!");
                break;
            }

            if (!_charactersDatabase.Values.TryGetValue(characterType, out var characterData))
            {
                Debug.LogError($"Character {characterType} is missing from characters database!");
                continue;
            }
            
            var character = Instantiate(characterData.CharacterPrefab);
            var pos = new CharacterPosition(0, slotIndex, ZoneType.Right);
            character.Initialize(characterData, pos);
            character.transform.position = _gameField.GetSlotPosition(pos);
            _enemyTeamLayout[pos] = character;
            
            slotIndex++;
        }
    }

    private async UniTask FightPhaseRoutine()
    {
        var floorsCount = _gameField.Floors.Length;
        for (int floorIndex = floorsCount - 1; floorIndex >= 0; floorIndex--)
        {
            if (GetAllAliveCharactersOnTheFloor(floorIndex).Count == 0)
            {
                continue;
            }
            
            UpdateCameraPosition(floorIndex);
            
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
            
            await HandleFloorFight(floorIndex);
            
            if (IsGameLost())
            {
                await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
                return;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        }

        _currentWaveIndex++;
        if (_currentWaveIndex < WavesConfig.Waves.Count)
        {
            var wave = WavesConfig.Waves[_currentWaveIndex];
            await SpawnWave(wave);
        }
    }
    
    private async UniTask HandleFloorFight(int floorIndex)
    {
        var enemiesOnTheFloor = GetCharactersOnTheFloor(floorIndex, ZoneType.Right);
        var playerUnitsOnTheFloor = GetCharactersOnTheFloor(floorIndex, ZoneType.Left);
        var charactersOnTheFloor = enemiesOnTheFloor.Concat(playerUnitsOnTheFloor).ToList();

        if (charactersOnTheFloor.Count == 0)
        {
            return;
        }

        foreach (var character in enemiesOnTheFloor)
        {
            if (character.IsDead)
            {
                continue;
            }

            try
            {
                await character.HandleMainAbility(charactersOnTheFloor);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            await UniTask.WaitUntil(() => !charactersOnTheFloor.Any(c => c != null && c.IsHandlingInProgress));
        }
        
        foreach (var character in playerUnitsOnTheFloor)
        {
            if (character.IsDead)
            {
                continue;
            }

            try
            {
                await character.HandleMainAbility(charactersOnTheFloor);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            await UniTask.WaitUntil(() => !charactersOnTheFloor.Any(c => c != null && c.IsHandlingInProgress));
        }

        CleanUpFloor(floorIndex);

        var aliveCharactersOnTheFloor = GetAllAliveCharactersOnTheFloor(floorIndex);
        foreach (var character in aliveCharactersOnTheFloor)
        {
            try
            {
                await character.HandleOnFloorFightEndAbility(aliveCharactersOnTheFloor);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            await UniTask.WaitUntil(() => !charactersOnTheFloor.Any(c => c != null && c.IsHandlingInProgress));
        }
        
        await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);

        await MoveEnemiesToTheNextFloor(floorIndex);
    }

    public List<Character> GetCharactersOnTheFloor(int floorIndex, ZoneType side)
    {
        var layout = side == ZoneType.Left ? _playerTeamLayout : _enemyTeamLayout;
        return layout.Values.Where(character => character != null && character.LayoutPos.floorIndex == floorIndex)
                .OrderBy(character => character.LayoutPos.zoneIndex)
                .ToList();
    }
    
    public List<Character> GetAllAliveCharactersOnTheFloor(int floorIndex)
    {
        return _playerTeamLayout.Values.Concat(_enemyTeamLayout.Values)
            .Where(
                character => character != null 
                             && !character.IsDead 
                             && character.LayoutPos.floorIndex == floorIndex)
            .OrderBy(character => character.LayoutPos.zoneIndex)
            .ToList();
    }
    
    public List<Character> GetAllAliveEnemies()
    {
        return _enemyTeamLayout.Values
            .Where(
                character => character != null && !character.IsDead)
            .ToList();
    }

    private async UniTask MoveEnemiesToTheNextFloor(int floorIndex)
    {
        var enemiesOnTheFloor = GetCharactersOnTheFloor(floorIndex, ZoneType.Right);
        if (enemiesOnTheFloor.Count == 0)
        {
            return;
        }

        if (floorIndex >= _gameField.Floors.Length - 1)
        {
            await HandleCoreDamage(enemiesOnTheFloor);
        }
        else
        {
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
            
            foreach (var character in enemiesOnTheFloor)
            {
                var currentPos = character.LayoutPos;
                _enemyTeamLayout[currentPos] = null;
                currentPos.floorIndex++;
                character.SetLayoutPos(currentPos);
                character.transform.position = _gameField.GetSlotPosition(currentPos);
                if (!_enemyTeamLayout.ContainsKey(currentPos))
                {
                    _enemyTeamLayout.Add(currentPos, null);
                }

                _enemyTeamLayout[currentPos] = character;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        }
    }

    private async UniTask HandleCoreDamage(List<Character> charactersList)
    {
        UpdateCameraPosition(_gameField.Floors.Length);
        
        foreach (var character in charactersList)
        {
            character.transform.position = _kingAttackPos.position;
            
            await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
            
            while (!character.IsDead)
            {
                await character.MainAbilityAnimation();
                CoreHp -= character.Power;

                if (IsGameLost())
                {
                    break;
                }
                
                _floatingTextFactory.ShowFloatingText($"-{character.Power.ToString()}", _king.transform.position + Vector3.up * 2.5f, FloatingTextFactory.FloatingTextType.Red);
                await HandleKingAttack(character);
            }
            
            if (IsGameLost())
            {
                break;
            }

            _enemyTeamLayout[character.LayoutPos] = null;
        }
    }

    private async UniTask HandleKingAttack(Character character)
    {
        var animationSeq = DOTween.Sequence().SetAutoKill(false);
        animationSeq
            .Append(_king.transform.DOMoveX(_king.transform.position.x + 2f, STANDARD_DELAY / 2f))
            .Join(_king.transform.DORotate(new Vector3(0f, 0f, -20f), STANDARD_DELAY / 2f))
            .OnComplete(() =>
            {
                animationSeq
                    .SetAutoKill(true)
                    .PlayBackwards();
            });
        
        int sfxIndex = Random.Range(0, _kingHitSfx.Count);
        _soundSystem.PlayOneShot(_kingHitSfx[sfxIndex]);

        await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        
        character.Damage(_gameSettings.KingDamage);
        
        await UniTask.Delay(TimeSpan.FromSeconds(STANDARD_DELAY), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
    }

    private void CleanUpFloor(int floorIndex)
    {
        var playerCharactersToClean = _playerTeamLayout.Values
            .Where(character => character != null && character.LayoutPos.floorIndex == floorIndex && character.IsDead)
            .ToList();

        foreach (var character in playerCharactersToClean)
        {
            _playerTeamLayout[character.LayoutPos] = null;
        }
        
        var enemyCharactersToClean = _enemyTeamLayout.Values
            .Where(character => character != null && character.LayoutPos.floorIndex == floorIndex && character.IsDead)
            .ToList();

        foreach (var character in enemyCharactersToClean)
        {
            _enemyTeamLayout[character.LayoutPos] = null;
        }
    }

    private bool ShouldProceedToFightPhase() => _currentBattlePhase == BattlePhase.Fight;

    private void InitializeWithData()
    {
        var wavesConfig = _gameSettings.GetWaveConfigForStage(_gameplayPersistentData.currentStage);
        WavesConfig = wavesConfig;
        
        var characterDatas = new List<CharacterData>();
        var availableCharacters = _gameplayPersistentData.charactersPool;
        if (availableCharacters.Count == 0)
        {
            availableCharacters = _gameSettings.GetInitialCharacters();
            _gameplayPersistentData.charactersPool.AddRange(availableCharacters);
        }

        foreach (var characterType in availableCharacters)
        {
            if (!_charactersDatabase.Values.TryGetValue(characterType, out var characterData))
            {
                Debug.LogError($"Character with type {characterType} missing from DB");
                continue;
            }
            
            characterDatas.Add(characterData);
        }

        _battleUi.SetupDraggablesPool(characterDatas);
    }

    [CanBeNull]
    public Character GetSlotCharacter(ZoneHitData zoneHitData)
    {
        var isPlayerTeamSlot = zoneHitData.zoneType == ZoneType.Left;
        var layoutDict = isPlayerTeamSlot ? _playerTeamLayout : _enemyTeamLayout;
        var pos = new CharacterPosition(zoneHitData);
        if (!layoutDict.TryGetValue(pos, out var character))
        {
            return null;
        }

        return character;
    }

    public bool PlacePlayerCharacter(CharacterData characterData, ZoneHitData zoneHitData)
    {
        if (!zoneHitData.zoneHit || zoneHitData.zoneType != ZoneType.Left || GetSlotCharacter(zoneHitData) != null || characterData.Cost > ManaPoints)
        {
            return false;
        }

        var character = Instantiate(characterData.CharacterPrefab);
        var pos = new CharacterPosition(zoneHitData);
        character.Initialize(characterData, pos);
        character.transform.position = _gameField.GetSlotPosition(pos);

        _playerTeamLayout[pos] = character;

        ManaPoints -= characterData.Cost;

        int sfxIndex = Random.Range(0, _placeAudio.Count);
        _soundSystem.PlayOneShot(_placeAudio[sfxIndex]);

        return true;
    }

    public bool CanPlacePlayerCharacter(ZoneHitData zoneHitData)
    {
        if (!zoneHitData.zoneHit || zoneHitData.zoneType != ZoneType.Left || GetSlotCharacter(zoneHitData) != null)
        {
            return false;
        }

        return true;
    }

    public bool TryApplyAbility(PlayerAbilityData abilityData, ZoneHitData zoneHitData)
    {
        var target = abilityData.target;
        if (!zoneHitData.zoneHit || target == PlayerAbilityTarget.PlayerUnit && zoneHitData.zoneType != ZoneType.Left ||
            target == PlayerAbilityTarget.EnemyUnit && zoneHitData.zoneType != ZoneType.Right)
        {
            return false;
        }
        
        var pos = new CharacterPosition(zoneHitData);
        var abilitySelectedTarget =
            target == PlayerAbilityTarget.PlayerUnit ? _playerTeamLayout[pos] : _enemyTeamLayout[pos];
        if (abilitySelectedTarget == null || abilitySelectedTarget.IsDead)
        {
            return false;
        }

        switch (abilityData.type)
        {
            case PlayerAbilityType.Damage:
                ManaPoints -= abilityData.Cost;
                abilitySelectedTarget.Damage(abilityData.Power);
                if (abilityData.abilityAudio != null)
                {
                    _soundSystem.PlayOneShot(abilityData.abilityAudio);
                }
                return true;
            
            case PlayerAbilityType.Heal:
                ManaPoints -= abilityData.Cost;
                abilitySelectedTarget.Heal(abilityData.Power);
                if (abilityData.abilityAudio != null)
                {
                    _soundSystem.PlayOneShot(abilityData.abilityAudio);
                }
                return true;
            
            default:
                return false;
        }
    }
}

[Serializable]
public class WaveConfig
{
    public List<string> Wave;
}

public struct CharacterPosition
{
    public int floorIndex;
    public ZoneType zoneType;
    public int zoneIndex;

    public CharacterPosition(int floorIndex, int zoneIndex, ZoneType zoneType)
    {
        this.floorIndex = floorIndex;
        this.zoneIndex = zoneIndex;
        this.zoneType = zoneType;
    }
    
    public CharacterPosition(ZoneHitData zoneHitData)
    {
        this.floorIndex = zoneHitData.floorIndex;
        this.zoneIndex = zoneHitData.zoneIndex;
        this.zoneType = zoneHitData.zoneType;
    }
}
