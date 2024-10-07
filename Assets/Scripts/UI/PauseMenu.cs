using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Canvas _pauseMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!_pauseMenu.enabled)
            {
                OpenMenu();
            }
            else
            {
                CloseMenu();
            }
        }
    }

    private void OpenMenu()
    {
        _pauseMenu.enabled = true;
    }
    
    private void CloseMenu()
    {
        _pauseMenu.enabled = false;
    }
}
