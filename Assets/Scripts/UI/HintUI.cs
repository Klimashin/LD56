using TMPro;
using UnityEngine;

public class HintUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private RectTransform _hintRect;

    public void Show(Vector2 position, string text)
    {
        _hintText.text = text;
        _hintRect.anchoredPosition = position;
        _hintRect.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _hintRect.gameObject.SetActive(false);
    }
}
