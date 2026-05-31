using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    public LoadingTransitionUi LoadingTransitionUi;

    private void Awake()
    {
        Instance = this;

        if (Instance.LoadingTransitionUi == null)
            Debug.LogError($"{typeof(LoadingTransitionUi)} component not assigned in inspector");
    }

    public static void ShowSceneTransitionUi(bool open)
    {
        if (open)
            Instance.LoadingTransitionUi.ShowUi();
        else
            Instance.LoadingTransitionUi.HideUi();
    }
}
