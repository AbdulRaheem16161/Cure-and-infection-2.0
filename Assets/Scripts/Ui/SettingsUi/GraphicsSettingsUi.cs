using UnityEngine;
using UnityEngine.UI;
using static UiManager;

public class GraphicsSettingsUi : MonoBehaviour, IUiPanel
{
    public GameObject settingsUi;

    public Button backButton;

    #region Blocked Inputs for Ui Screen api
    public InputManager.InputBlock GetInputBlock()
    {
        return InputManager.InputBlock.Look | InputManager.InputBlock.Move | InputManager.InputBlock.Combat;
    }
    #endregion

    #region Initialize Ui + Button Listeners
    private void Start()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        backButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.graphicsSettings)));
    }

    private void OnDestroy()
    {
        backButton.onClick.RemoveAllListeners();
    }
    #endregion

    #region Show/Hide Ui Api
    public void ShowUi(UiContext uiContext)
    {
        settingsUi.SetActive(true);
    }
    public void HideUi()
    {
        settingsUi.SetActive(false);
    }
    #endregion
}
