using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
    public GameObject mainMenuUi;

    public void ShowUi()
    {
        mainMenuUi.SetActive(true);
    }
    public void HideUi()
    {
        mainMenuUi.SetActive(false);
    }
}
