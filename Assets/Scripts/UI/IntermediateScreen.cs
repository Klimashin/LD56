using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntermediateScreen : MonoBehaviour
{
    [SerializeField] private StageImage[] stageImages;
    [SerializeField] private float minLoadTime = 2f;

    private GameplayPersistentData _gameplayPersistentData;

    [Inject]
    private void Inject(GameplayPersistentData gameplayPersistentData)
    {
        _gameplayPersistentData = gameplayPersistentData;
    }

    private void Start()
    {
        IntermediateRoutine().Forget();
    }

    private async UniTaskVoid IntermediateRoutine()
    {
        var currentStage = _gameplayPersistentData.currentStage;
        for (int i = 0; i < stageImages.Length; i++)
        {
            bool isSelected = i == currentStage;
            bool isCompleted = i < currentStage;
            stageImages[i].Initialize(isSelected, isCompleted);
        }

        float loadTime = 0;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(2);
        
        asyncLoad.allowSceneActivation = false;
        
        while (!asyncLoad.isDone)
        {
            //scene has loaded as much as possible,
            // the last 10% can't be multi-threaded
            if (asyncLoad.progress >= 0.9f && loadTime >= minLoadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            loadTime += Time.unscaledDeltaTime;
            await UniTask.DelayFrame(1);
        }
    }
}
