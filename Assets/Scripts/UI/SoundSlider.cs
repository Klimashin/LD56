using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SoundSlider : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    
    private SoundSystem _soundSystem;
    
    [Inject]
    private void Inject(SoundSystem soundSystem)
    {
        _soundSystem = soundSystem;
    }

    private void Start()
    {
        _slider.value = _soundSystem.GetVolume();
        _slider.onValueChanged.AddListener(UpdateVolume);
    }

    private void UpdateVolume(float sliderValue)
    {
        _soundSystem.SetVolume(sliderValue);
    }
}
