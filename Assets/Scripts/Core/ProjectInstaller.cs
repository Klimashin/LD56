using Reflex.Core;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private SoundSystem _soundSystem;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(_soundSystem);
        containerBuilder.AddSingleton(new EventsDispatcher());
    }
}
