using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class SwithFloorButton : MonoBehaviour
{
    [SerializeField] private Image _arrowIcon;
    [SerializeField] private int _index;
    
    private BattleController _battleController;

    [Inject]
    private void Inject(BattleController battleController)
    {
        _battleController = battleController;
    }

    private void Update()
    {
        bool isDown = _battleController.CurrentCameraWaypoint > _index;
        _arrowIcon.transform.localScale = new Vector3(1, isDown ? -1 : 1, 1);
    }
}
