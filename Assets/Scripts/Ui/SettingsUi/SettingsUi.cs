using UnityEngine;

public class SettingsUi : MonoBehaviour, IUiPanel
{
    public GameObject settingsUi;

    public void ShowUi(UiContext uiContext)
    {
        settingsUi.SetActive(true);
    }
    public void HideUi()
    {
        settingsUi.SetActive(false);
    }
}
