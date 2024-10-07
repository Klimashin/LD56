using System.Collections.Generic;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerAbilityDraggable : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image raycastTarget;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _powerText;
    [SerializeField] private PlayerAbilityData _abilityData;
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private GameObject _hintTextTransform;
    
    private Vector3 _initialPos;
    private Physics2DRaycaster _physics2DRaycaster;
    private BattleController _battleController;
    private FieldPlaceholdersController _fieldPlaceholdersController;
    
    private readonly List<RaycastResult> _raycastResults = new ();

    [Inject]
    private void Inject(Physics2DRaycaster physics2DRaycaster, BattleController battleController, FieldPlaceholdersController fieldPlaceholdersController)
    {
        _physics2DRaycaster = physics2DRaycaster;
        _battleController = battleController;
        _fieldPlaceholdersController = fieldPlaceholdersController;
    }
    
    private void Start()
    {
        _costText.text = _abilityData.Cost.ToString();
        _powerText.text = _abilityData.Power.ToString();
        _hintText.text = _abilityData.Description;
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_battleController.ManaPoints < _abilityData.Cost)
        {
            return;
        }

        _initialPos = transform.position;
        _hintTextTransform.gameObject.SetActive(false);
        raycastTarget.raycastTarget = false;

        if (_abilityData.target == PlayerAbilityTarget.PlayerUnit)
        {
            _fieldPlaceholdersController.StartHighlightingOccupiedPlayerZones();
        }
        else
        {
            _fieldPlaceholdersController.StartHighlightingOccupiedEnemyZones();
        }
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_battleController.ManaPoints < _abilityData.Cost)
        {
            return;
        }
        
        _fieldPlaceholdersController.StopHighlightingZones();
        raycastTarget.raycastTarget = true;
        
        _raycastResults.Clear();
        _physics2DRaycaster.Raycast(eventData, _raycastResults);
            
        foreach (var raycastResult in _raycastResults)
        {
            if (raycastResult.gameObject.TryGetComponent<GameField>(out var gameField))
            {
                var zoneHitData = gameField.GetZoneHitData(raycastResult.worldPosition);
                if (_battleController.TryApplyAbility(_abilityData, zoneHitData))
                {
                    break;
                }
            }
        }
        
        transform.position = _initialPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_battleController.ManaPoints < _abilityData.Cost)
        {
            return;
        }
        
        transform.position = eventData.position;
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
