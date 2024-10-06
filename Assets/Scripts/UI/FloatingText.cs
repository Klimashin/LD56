using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private float _lifetime = 1f;
    [SerializeField] private float floatDistance = 1f;

    public void Initialize(string text)
    {
        _text.text = text;

        transform
            .DOMoveY(transform.position.y + floatDistance, _lifetime)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
}
