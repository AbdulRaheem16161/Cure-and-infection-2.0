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


    #region Initialize Ui + Button Listeners
    private void Start()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        gameplaySettingsButton.onClick.AddListener(() => ShowScreen(new(UiScreens.gameplaySettings)));
        controlSettingsButton.onClick.AddListener(() => ShowScreen(new(UiScreens.controlSettings)));
        GraphicsSettingsButton.onClick.AddListener(() => ShowScreen(new(UiScreens.graphicsSettings)));
        audioSettingsButton.onClick.AddListener(() => ShowScreen(new(UiScreens.audioSettings)));
        backButton.onClick.AddListener(() => ShowScreen(new(UiScreens.settings)));
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
