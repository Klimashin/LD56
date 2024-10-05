using Reflex.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameplayInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private Physics2DRaycaster _physics2DRaycaster;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private BattleUI _battleUI;
    [SerializeField] private CharactersDatabase _charactersDatabase;
    [SerializeField] private GameField _gameField;
    [SerializeField] private BattleController _battleController; 
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(_physics2DRaycaster);
        containerBuilder.AddSingleton(_mainCamera);
        containerBuilder.AddSingleton(_battleUI);
        containerBuilder.AddSingleton(_charactersDatabase);
        containerBuilder.AddSingleton(_gameField);
        containerBuilder.AddSingleton(_battleController);
    }
}
