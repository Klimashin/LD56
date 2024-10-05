using System;
using System.Collections.Generic;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Transform _draggablesTransform;
    [SerializeField] private GameObject _toyBox;
    [SerializeField] private Button _deployFinishedButton;
    [SerializeField] private TextMeshProUGUI _coreHpText;

    private BattleController _battleController;
    
    public Action onDeployFinishedClickedAction;

    [Inject]
    private void Inject(BattleController battleController)
    {
        _battleController = battleController;
    }

    private void Start()
    {
        _deployFinishedButton.onClick.AddListener(OnDeployFinished);
    }

    private void Update()
    {
        _toyBox.gameObject.SetActive(_battleController.CurrentBattlePhase == BattleController.BattlePhase.Deploy);
        _deployFinishedButton.gameObject.SetActive(_battleController.CurrentBattlePhase == BattleController.BattlePhase.Deploy);
        _coreHpText.text = _battleController.CoreHp.ToString();
    }

    private void OnDeployFinished()
    {
        onDeployFinishedClickedAction.Invoke();
    }

    private readonly List<ToyBoxDraggableItem> _draggableItems = new ();

    public void SetupDraggablesPool(List<CharacterData> characterDatas)
    {
        foreach (var toyBoxDraggableItem in _draggableItems)
        {
            Destroy(toyBoxDraggableItem.gameObject);
        }
        
        _draggableItems.Clear();

        foreach (var characterData in characterDatas)
        {
            var draggable = Instantiate(characterData.CharacterUiDraggablePrefab, _draggablesTransform);
            draggable.SetCharacterData(characterData);
            _draggableItems.Add(draggable);
        }
    }
}
