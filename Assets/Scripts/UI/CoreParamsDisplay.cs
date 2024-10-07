using Reflex.Attributes;
using TMPro;
using UnityEngine;

public class CoreParamsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _powerText;

    private BattleController _battleController;
    private GameSettings _gameSettings;
    
    [Inject]
    private void Inject(BattleController battleController, GameSettings gameSettings)
    {
        _battleController = battleController;
        _gameSettings = gameSettings;
    }

    private void Update()
    {
        _hpText.text = _battleController.CoreHp.ToString();
        _powerText.text = _gameSettings.KingDamage.ToString();
    }
}
