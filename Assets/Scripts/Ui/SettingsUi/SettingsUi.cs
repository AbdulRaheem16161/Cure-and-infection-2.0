using UnityEngine;

public class SettingsUi : MonoBehaviour
{
    public GameObject settingsUi;

    public void ShowUi()
    {
        settingsUi.SetActive(true);
    }
    public void HideUi()
    {
        settingsUi.SetActive(false);
    }
}
