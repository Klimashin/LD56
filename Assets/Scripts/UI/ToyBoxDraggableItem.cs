using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToyBoxDraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Image raycastTarget;
    
    private Vector3 _initialPos;
    private Physics2DRaycaster _physics2DRaycaster;
    private BattleController _battleController;
    private FieldPlaceholdersController _fieldPlaceholdersController;
    private CharacterData _characterData;

    private readonly List<RaycastResult> _raycastResults = new ();

    [Inject]
    private void Inject(Physics2DRaycaster physics2DRaycaster, BattleController battleController, FieldPlaceholdersController fieldPlaceholdersController)
    {
        _physics2DRaycaster = physics2DRaycaster;
        _battleController = battleController;
        _fieldPlaceholdersController = fieldPlaceholdersController;
    }

    public void SetCharacterData(CharacterData characterData)
    {
        _characterData = characterData;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _initialPos = transform.position;
        raycastTarget.raycastTarget = false;
        GetComponent<LayoutElement>().ignoreLayout = true;
        _fieldPlaceholdersController.StartHighlightingFreePlayerZones();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _fieldPlaceholdersController.StopHighlightingFreePlayerZones();
        GetComponent<LayoutElement>().ignoreLayout = false;
        raycastTarget.raycastTarget = true;
        
        _raycastResults.Clear();
        _physics2DRaycaster.Raycast(eventData, _raycastResults);
            
        foreach (var raycastResult in _raycastResults)
        {
            if (raycastResult.gameObject.TryGetComponent<GameField>(out var gameField))
            {
                var zoneHitData = gameField.GetZoneHitData(raycastResult);
                Debug.Log($"GameField was hit! HitData: {zoneHitData}");
                if (_battleController.PlacePlayerCharacter(_characterData, zoneHitData))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
        
        transform.position = _initialPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        
        _raycastResults.Clear();
        _physics2DRaycaster.Raycast(eventData, _raycastResults);
            
        foreach (var raycastResult in _raycastResults)
        {
            if (raycastResult.gameObject.TryGetComponent<GameField>(out var gameField))
            {
                var zoneHitData = gameField.GetZoneHitData(raycastResult);
                if (!zoneHitData.zoneHit)
                {
                    continue;
                }

                if (_battleController.CanPlacePlayerCharacter(zoneHitData))
                {
                    // highlight
                }
            }
        }
    }
}
