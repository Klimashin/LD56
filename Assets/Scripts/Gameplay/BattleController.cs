using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class BattleController : MonoBehaviour, IEventsDispatcherClient
{
    [SerializeField] private List<CharacterType> _availableCharacters;
    
    public List<Character> PlayerTeam { get; } = new();
    public List<Character> EnemyTeam { get; } = new();

    private BattleUI _battleUi;
    private GameField _gameField;
    private CharactersDatabase _charactersDatabase;

    [Inject]
    private void Inject(BattleUI battleUI, CharactersDatabase charactersDatabase, GameField gameField)
    {
        _battleUi = battleUI;
        _charactersDatabase = charactersDatabase;
        _gameField = gameField;
    }

    private void Start()
    {
        InitializeWithTestData();
    }

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

    public bool PlacePlayerCharacter(CharacterData characterData, ZoneHitData zoneHitData)
    {
        if (!zoneHitData.zoneHit || zoneHitData.zoneType != ZoneType.Left)
        {
            return false;
        }

        var character = Instantiate(characterData.CharacterPrefab);
        character.transform.position =
            _gameField.Floors[zoneHitData.floorIndex].LeftZone.GetSlotPosition(zoneHitData.zoneIndex);

        return true;
    }

    public bool CanPlacePlayerCharacter(CharacterData characterData, ZoneHitData zoneHitData)
    {
        if (!zoneHitData.zoneHit || zoneHitData.zoneType != ZoneType.Left)
        {
            return false;
        }

        return true;
    }
}
