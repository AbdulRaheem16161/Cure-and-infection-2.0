using UnityEngine;

public class PauseUi : MonoBehaviour
{
    public GameObject pauseUi;

    public void ShowUi()
    {
        pauseUi.SetActive(true);
    }
    public void HideUi()
    {
        pauseUi.SetActive(false);
    }
}
