using Reflex.Attributes;
using UnityEngine;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private AudioClip _BgMusic;

    private SoundSystem _soundSystem;
    
    [Inject]
    private void Inject(SoundSystem soundSystem, GameplayPersistentData gameplayPersistentData, GameSettings gameSettings)
    {
        _soundSystem = soundSystem;
    }

    private void Start()
    {
        _soundSystem.PlayMusicClip(_BgMusic);
    }
}
