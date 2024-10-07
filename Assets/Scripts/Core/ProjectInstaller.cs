using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private SoundSystem _soundSystem;
    [SerializeField] private GameSettings _gameSettings;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(_soundSystem);
        containerBuilder.AddSingleton(new EventsDispatcher());
        containerBuilder.AddSingleton(_gameSettings);
        containerBuilder.AddSingleton(new GameplayPersistentData());
    }
}
