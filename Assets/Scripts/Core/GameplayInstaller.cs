using Cinemachine;
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
    [SerializeField] private FloatingTextFactory _floatingTextFactory;
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCamera;
    [SerializeField] private FieldPlaceholdersController _fieldPlaceholdersController;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(_physics2DRaycaster);
        containerBuilder.AddSingleton(_mainCamera);
        containerBuilder.AddSingleton(_battleUI);
        containerBuilder.AddSingleton(_charactersDatabase);
        containerBuilder.AddSingleton(_gameField);
        containerBuilder.AddSingleton(_battleController);
        containerBuilder.AddSingleton(_floatingTextFactory);
        containerBuilder.AddSingleton(_cinemachineVirtualCamera);
        containerBuilder.AddSingleton(_fieldPlaceholdersController);
    }
}
