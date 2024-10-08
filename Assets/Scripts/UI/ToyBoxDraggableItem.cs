using System.Collections.Generic;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToyBoxDraggableItem : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image raycastTarget;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private GameObject _hintTextTransform;
    
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
        _costText.text = characterData.Cost.ToString();
        _hintText.text = characterData.Description;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        _hintTextTransform.gameObject.SetActive(false);
        
        if (_battleController.ManaPoints < _characterData.Cost)
        {
            return;
        }

        _initialPos = transform.position;
        raycastTarget.raycastTarget = false;
        GetComponent<LayoutElement>().ignoreLayout = true;
        _fieldPlaceholdersController.StartHighlightingFreePlayerZones();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_battleController.ManaPoints < _characterData.Cost)
        {
            return;
        }
        
        _fieldPlaceholdersController.StopHighlightingZones();
        GetComponent<LayoutElement>().ignoreLayout = false;
        raycastTarget.raycastTarget = true;
        
        _raycastResults.Clear();
        _physics2DRaycaster.Raycast(eventData, _raycastResults);
            
        foreach (var raycastResult in _raycastResults)
        {
            if (raycastResult.gameObject.TryGetComponent<GameField>(out var gameField))
            {
                var zoneHitData = gameField.GetZoneHitData(raycastResult.worldPosition);
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
        if (_battleController.ManaPoints < _characterData.Cost)
        {
            return;
        }
        
        transform.position = eventData.position;
        
        _raycastResults.Clear();
        _physics2DRaycaster.Raycast(eventData, _raycastResults);
            
        foreach (var raycastResult in _raycastResults)
        {
            if (raycastResult.gameObject.TryGetComponent<GameField>(out var gameField))
            {
                var zoneHitData = gameField.GetZoneHitData(raycastResult.worldPosition);
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hintTextTransform.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hintTextTransform.gameObject.SetActive(false);
    }
}
