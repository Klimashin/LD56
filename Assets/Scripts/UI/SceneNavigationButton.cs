using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneNavigationButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private int _sceneIndex;

    private void Start()
    {
        _button.onClick.AddListener(() => SceneManager.LoadScene(_sceneIndex));
    }
}
