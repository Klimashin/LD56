using System;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSlotUi : MonoBehaviour
{
    [SerializeField] private Button _button;

    public Action OnSlotClickAction;

    private void Awake()
    {
        _button.onClick.AddListener(OnSlotClick);
    }

    public void SetOnSlotClickAction(Action action)
    {
        OnSlotClickAction = action;
    }

    private void OnSlotClick()
    {
        OnSlotClickAction?.Invoke();
    }
}
