using Reflex.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterHintRaycastZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _hintText;
    [SerializeField] private GameObject _hintTextTransform;

    public void Initialize(string characterDataDescription)
    {
        _hintText.text = characterDataDescription;
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
