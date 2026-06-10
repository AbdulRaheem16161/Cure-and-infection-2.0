using UnityEngine;
using UnityEngine.UI;
using static UiManager;

public class AudioSettingsUi : MonoBehaviour, IUiPanel
{
    public GameObject settingsUi;

    public Button backButton;


    #region Initialize Ui + Button Listeners
    private void Start()
    {
        InitializeUi();
    }

    private void InitializeUi()
    {
        backButton.onClick.AddListener(() => ShowScreen(new(UiScreens.audioSettings)));
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
