using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Reflex.Attributes;
using UnityEngine;

public class BattleController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private List<string> _availableCharacters;
    [SerializeField] private List<WaveConfig> _enemiesWaves;
    [SerializeField] private int _coreBaseHp = 15;

    private BattleUI _battleUi;
    private GameField _gameField;
    private CharactersDatabase _charactersDatabase;
    private readonly Dictionary<CharacterPosition, Character> _playerTeamLayout = new ();
    private readonly Dictionary<CharacterPosition, Character> _enemyTeamLayout = new ();
    private bool _winConditionReached = false;
    private BattlePhase _currentBattlePhase = BattlePhase.Deploy;
    private int _currentWaveIndex = -1;

    public BattlePhase CurrentBattlePhase => _currentBattlePhase;
    public int CoreHp { get; private set; }

    public enum BattlePhase
    {
        Deploy,
        Fight
    }

    [Inject]
    private void Inject(BattleUI battleUI, CharactersDatabase charactersDatabase, GameField gameField)
    {
        _battleUi = battleUI;
        _charactersDatabase = charactersDatabase;
        _gameField = gameField;
    }

    private void Start()
    {
        CoreHp = _coreBaseHp;
        
        _battleUi.onDeployFinishedClickedAction += () => { UpdateBattlePhase(BattlePhase.Fight); };
        
        InitializeWithTestData();

        BattleRoutine().Forget();
    }

    private async UniTaskVoid BattleRoutine()
    {
        _currentWaveIndex = 0;
        var wave = _enemiesWaves[_currentWaveIndex];
        await SpawnWave(wave);
        
        while (!_winConditionReached)
        {
            await UniTask.WaitUntil(ShouldProceedToFightPhase, PlayerLoopTiming.Update, destroyCancellationToken);
            
            await FightPhaseRoutine();

            UpdateBattlePhase(BattlePhase.Deploy);
        }
    }

    private void UpdateBattlePhase(BattlePhase phase)
    {
        _currentBattlePhase = phase;
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
            if (!_enemyTeamLayout.ContainsKey(pos))
            {
                _enemyTeamLayout.Add(pos, null);
            }

            _enemyTeamLayout[pos] = character;
            
            slotIndex++;
        }
    }

    private async UniTask FightPhaseRoutine()
    {
        var floorsCount = _gameField.Floors.Length;
        for (int floorIndex = floorsCount - 1; floorIndex >= 0; floorIndex--)
        {
            await HandleFloorFight(floorIndex);
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), DelayType.DeltaTime, PlayerLoopTiming.Update, destroyCancellationToken);
        }

        _currentWaveIndex++;
        if (_currentWaveIndex < _enemiesWaves.Count)
        {
            var wave = _enemiesWaves[_currentWaveIndex];
            await SpawnWave(wave);
        }
    }

    private async UniTask HandleFloorFight(int floorIndex)
    {
        var enemiesOnTheFloor = GetCharactersOnTheFloor(floorIndex, ZoneType.Right);
        if (enemiesOnTheFloor.Count == 0)
        {
            return;
        }

        var playerUnitsOnTheFloor = GetCharactersOnTheFloor(floorIndex, ZoneType.Left);
        
        // handle haste units

        foreach (var character in enemiesOnTheFloor)
        {
            await character.HandleAttack(playerUnitsOnTheFloor);
        }
        
        foreach (var character in playerUnitsOnTheFloor)
        {
            if (character.IsDead)
            {
                continue;
            }
            
            await character.HandleAttack(enemiesOnTheFloor);
        }

        CleanUpFloor(floorIndex);

        await MoveEnemiesToTheNextFloor(floorIndex);
    }

    private List<Character> GetCharactersOnTheFloor(int floorIndex, ZoneType side)
    {
        var layout = side == ZoneType.Left ? _playerTeamLayout : _enemyTeamLayout;
        return layout.Values.Where(character => character != null && character.LayoutPos.floorIndex == floorIndex)
                .OrderBy(character => character.LayoutPos.zoneIndex)
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
        }
    }

    private async UniTask HandleCoreDamage(List<Character> charactersList)
    {
        foreach (var character in charactersList)
        {
            CoreHp -= character.AttackPower;
            _enemyTeamLayout[character.LayoutPos] = null;
            Destroy(character.gameObject);
        }
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
            _playerTeamLayout[character.LayoutPos] = null;
        }
    }

    private bool ShouldProceedToFightPhase() => _currentBattlePhase == BattlePhase.Fight;

    private void InitializeWithTestData()
    {
        var characterDatas = new List<CharacterData>();
        foreach (var characterType in _availableCharacters)
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
        if (!zoneHitData.zoneHit || zoneHitData.zoneType != ZoneType.Left || GetSlotCharacter(zoneHitData) != null)
        {
            return false;
        }

        var character = Instantiate(characterData.CharacterPrefab);
        var pos = new CharacterPosition(zoneHitData);
        character.Initialize(characterData, pos);
        character.transform.position = _gameField.GetSlotPosition(pos);

        if (!_playerTeamLayout.ContainsKey(pos))
        {
            _playerTeamLayout.Add(pos, null);
        }

        _playerTeamLayout[pos] = character;

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
