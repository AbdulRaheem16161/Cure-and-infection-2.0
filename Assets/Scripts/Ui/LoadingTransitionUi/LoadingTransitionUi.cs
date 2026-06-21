using UnityEngine;

public class LoadingTransitionUi : MonoBehaviour, IUiPanel
{
    public GameObject loadingSceneTransitionUi;
    public ProgressBarUi loadingBarUi;

    #region Blocked Inputs for Ui Screen api
    public InputManager.InputBlock GetInputBlock()
    {
        return InputManager.InputBlock.Look | InputManager.InputBlock.Move | InputManager.InputBlock.Combat | InputManager.InputBlock.UI;
    }
    #endregion

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
        loadingBarUi.UpdateSceneTransitionLoadingText(loading, sceneName, ProgressBarUi.ScaleAxis.x, progress);
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
