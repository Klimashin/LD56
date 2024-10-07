using UnityEngine;
using UnityEngine.UI;

public class StageImage : MonoBehaviour
{
    [SerializeField] private Image _stageSelectedImage;
    [SerializeField] private Image _stageCompletedImage;

    public void Initialize(bool isSelected, bool isCompleted)
    {
        _stageCompletedImage.enabled = isCompleted;
        _stageSelectedImage.enabled = isSelected;
    }
}
