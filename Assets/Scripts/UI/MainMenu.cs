using Reflex.Attributes;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioClip _menuBgMusic;

    private SoundSystem _soundSystem;
    
    [Inject]
    private void Inject(SoundSystem soundSystem)
    {
        _soundSystem = soundSystem;
    }
    
    private void Start()
    {
        if (!_soundSystem.IsPlayingClip(_menuBgMusic))
        {
            _soundSystem.PlayMusicClip(_menuBgMusic);
        }
    }
}
