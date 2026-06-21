using UnityEngine;
using UnityEngine.UI;
using static UiManager;

public class SettingsUi : MonoBehaviour, IUiPanel
{
    public GameObject settingsUi;

    public Button gameplaySettingsButton;
    public Button controlSettingsButton;
    public Button GraphicsSettingsButton;
    public Button audioSettingsButton;
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
        gameplaySettingsButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.gameplaySettings)));
        controlSettingsButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.controlSettings)));
        GraphicsSettingsButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.graphicsSettings)));
        audioSettingsButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.audioSettings)));
        backButton.onClick.AddListener(() => ToggleScreen(new(UiScreens.settings)));
    }

    private void OnDestroy()
    {
        gameplaySettingsButton.onClick.RemoveAllListeners();
        controlSettingsButton.onClick.RemoveAllListeners();
        GraphicsSettingsButton.onClick.RemoveAllListeners();
        audioSettingsButton.onClick.RemoveAllListeners();
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
