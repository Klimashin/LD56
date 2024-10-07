using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;

public class FieldPlaceholdersController : MonoBehaviour
{
    [SerializeField] private GameObject _fieldPlaceholderPrefab;

    private BattleController _battleController;
    private GameField _gameField;

    private readonly Dictionary<CharacterPosition, GameObject> _placeholders = new ();

    [Inject]
    private void Inject(BattleController battleController, GameField gameField)
    {
        _battleController = battleController;
        _gameField = gameField;
    }

    private void Start()
    {
        for (var floorIndex = 0; floorIndex < _gameField.Floors.Length; floorIndex++)
        {
            for (int slotIndex = 0; slotIndex < Floor.FLOOR_SLOTS_COUNT; slotIndex++)
            {
                var positionLeft = new CharacterPosition(floorIndex, slotIndex, ZoneType.Left);
                var positionRight = new CharacterPosition(floorIndex, slotIndex, ZoneType.Right);
                var placeholderLeft = Instantiate(_fieldPlaceholderPrefab);
                placeholderLeft.transform.position = _gameField.GetSlotPosition(positionLeft);
                var placeholderRight = Instantiate(_fieldPlaceholderPrefab);
                placeholderRight.transform.position = _gameField.GetSlotPosition(positionRight);
                _placeholders.Add(positionLeft, placeholderLeft);
                _placeholders.Add(positionRight, placeholderRight);
                placeholderLeft.gameObject.SetActive(false);
                placeholderRight.gameObject.SetActive(false);
            }
        }
    }

    public void StartHighlightingFreePlayerZones()
    {
        var freeZones = _battleController.GetFreePlayerZones();
        foreach (var characterPosition in freeZones)
        {
            _placeholders[characterPosition].gameObject.SetActive(true);   
        }
    }
    
    public void StartHighlightingOccupiedPlayerZones()
    {
        var zones = _battleController.GetOccupiedPlayerZones();
        foreach (var characterPosition in zones)
        {
            _placeholders[characterPosition].gameObject.SetActive(true);   
        }
    }
    
    public void StartHighlightingOccupiedEnemyZones()
    {
        var zones = _battleController.GetOccupiedEnemyZones();
        foreach (var characterPosition in zones)
        {
            _placeholders[characterPosition].gameObject.SetActive(true);   
        }
    }
    
    public void StopHighlightingZones()
    {
        foreach (var characterPosition in _placeholders.Keys)
        {
            _placeholders[characterPosition].gameObject.SetActive(false);   
        }
    }
}
