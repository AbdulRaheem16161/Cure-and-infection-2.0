using UnityEngine;

public class LoadingTransitionUi : MonoBehaviour, IUiPanel
{
    public GameObject loadingSceneTransitionUi;
    public LoadingBarUi loadingBarUi;

    private void OnEnable()
    {
        SceneHandler.OnSceneTransitionProgress += OnSceneTransitionProgress;
    }
    private void OnDisable()
    {
        SceneHandler.OnSceneTransitionProgress -= OnSceneTransitionProgress;
    }

    private void OnSceneTransitionProgress(bool loading, string sceneName, float progress)
    {
        loadingBarUi.UpdateSceneTransitionLoadingText(loading, sceneName, progress);
        loadingBarUi.UpdateBarProgress(LoadingBarUi.ScaleAxis.x, progress);
    }

    public void ShowUi(UiContext uiContext)
    {
        loadingSceneTransitionUi.SetActive(true);
    }
    public void HideUi()
    {
        loadingSceneTransitionUi.SetActive(false);
    }
}
