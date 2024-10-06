using System;
using UnityEngine;

public class FloatingTextFactory : MonoBehaviour
{
    [SerializeField] private FloatingText _floatingTextRedPrefab;
    [SerializeField] private FloatingText _floatingTextGreenPrefab;

    public enum FloatingTextType
    {
        Red,
        Green
    }

    public void ShowFloatingText(string text, Vector3 worldPos, FloatingTextType floatingTextType)
    {
        FloatingText floatingText;
        switch (floatingTextType)
        {
            case FloatingTextType.Red:
                floatingText = Instantiate(_floatingTextRedPrefab);
                break;
            case FloatingTextType.Green:
                floatingText = Instantiate(_floatingTextGreenPrefab);
                break;
            default:
                throw new Exception();
        }
        
        floatingText.transform.position = worldPos;
        floatingText.Initialize(text);
    }
}
