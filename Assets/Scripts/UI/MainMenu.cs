using Reflex.Attributes;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private AudioClip _menuBgMusic;

    private SoundSystem _soundSystem;
    private GameplayPersistentData _gameplayPersistentData;
    private GameSettings _gameSettings;
    
    [Inject]
    private void Inject(SoundSystem soundSystem, GameplayPersistentData gameplayPersistentData, GameSettings gameSettings)
    {
        _soundSystem = soundSystem;
        _gameplayPersistentData = gameplayPersistentData;
        _gameSettings = gameSettings;
    }
    
    private void Start()
    {
        if (!_soundSystem.IsPlayingClip(_menuBgMusic))
        {
            _soundSystem.PlayMusicClip(_menuBgMusic);
        }

        _gameplayPersistentData.currentStage = 0;
        _gameplayPersistentData.charactersPool.Clear();
        _gameplayPersistentData.charactersPool.AddRange(_gameSettings.GetInitialCharacters());
    }
}
