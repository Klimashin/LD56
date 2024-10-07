using System.Collections.Generic;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private Transform _upgradeSlotsTransform;

    private GameplayPersistentData _gameplayPersistentData;

    [Inject]
    private void Inject(GameplayPersistentData gameplayPersistentData)
    {
        _gameplayPersistentData = gameplayPersistentData;
    }

    public void Show()
    {
        GetComponent<Canvas>().enabled = true;
    }

    public void Initialize(List<UpgradePackageConfig> upgradePackageConfigs)
    {
        foreach (var upgradePackageConfig in upgradePackageConfigs)
        {
            var slotPrefab = upgradePackageConfig.UpgradeSlotUi;
            var slot = Instantiate(slotPrefab, _upgradeSlotsTransform);
            var package = upgradePackageConfig;
            slot.SetOnSlotClickAction(() => OnPackageSelected(package));
        }
    }

    private void OnPackageSelected(UpgradePackageConfig package)
    {
        _gameplayPersistentData.charactersPool.AddRange(package.CharactersInPackage);
        if (_gameplayPersistentData.currentStage < GameSettings.MAX_STAGE_INDEX)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.LogError("Upgrade UI is shown after final stage already finished!");
        }
    }
}
