using TMPro;
using UnityEngine;

public class LoadingBarUi : MonoBehaviour
{
    public GameObject loadingBar;
    public RectTransform loadingBarProgressRectTransform;
    public TMP_Text loadingBarText;

    public enum ScaleAxis {x, y, z}

    public void UpdateSceneTransitionLoadingText(bool loading, string sceneName, float progress)
    {
        string text = loading ? "Loading " : "Unloading ";
        text += $"{sceneName} {progress * 100}%";
        loadingBarText.text = text;
    }

    public void UpdateBarProgress(ScaleAxis scaleAxis, float percentage)
    {
        switch (scaleAxis)
        {
            case ScaleAxis.x:
                loadingBarProgressRectTransform.localScale = new Vector3(percentage, 1f, 1f);
                break;
            case ScaleAxis.y:
                loadingBarProgressRectTransform.localScale = new Vector3(1f, percentage, 1f);
                break;
            case ScaleAxis.z:
                loadingBarProgressRectTransform.localScale = new Vector3(1f, 1f, percentage);
                break;
        }
    }
}
