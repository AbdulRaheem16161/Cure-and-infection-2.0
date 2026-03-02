using UnityEngine;

public class PauseManager : MonoBehaviour
{
    #region State
    private bool isPaused;
    #endregion

    private void Update()
    {
        #region Update
        CheckPauseInput();
        #endregion
    }

    void CheckPauseInput()
    {
        #region CheckPauseInput
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        #endregion
    }

    void TogglePause()
    {
        #region TogglePause
        isPaused = !isPaused;

        // Freeze or resume time
        Time.timeScale = isPaused ? 0f : 1f;

        // Optional: lock cursor logic here later
        #endregion
    }
}
