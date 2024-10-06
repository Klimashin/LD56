using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class FloorButtonsController : MonoBehaviour
{
    [SerializeField] private Button[] _buttons;

    private BattleController _battleController;
    
    [Inject]
    private void Inject(BattleController battleController)
    {
        _battleController = battleController;
    }

    private void Start()
    {
        for (var i = 0; i < _buttons.Length; i++)
        {
            int index = i;
            _buttons[i].onClick.AddListener(() => OnFloorButtonClick(index));
        }
    }

    private void OnFloorButtonClick(int index)
    {
        switch (index)
        {
            case 0:
                _battleController.UpdateCameraPosition(_battleController.CurrentCameraWaypoint == 0 ? 1 : 0);
                break;
            
            case 1:
                _battleController.UpdateCameraPosition(_battleController.CurrentCameraWaypoint == 1 ? 2 : 1);
                break;
            
            case 2:
                _battleController.UpdateCameraPosition(_battleController.CurrentCameraWaypoint == 2 ? 3 : 2);
                break;
        }
    }
}
