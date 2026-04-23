using UnityEngine;

public class LightingManager : MonoBehaviour
{
    [Header("Lights")]
    [SerializeField] private Light sun;
    [SerializeField] private Light moon;

    [Header("Preset")]
    [SerializeField] private LightingPreset preset;

    [Header("Time")]
    [SerializeField, Range(0, 24)] private float timeOfDay;
    [SerializeField] private float dayLengthMinutes = 5f;

    [Header("Moon Settings")]
    [SerializeField] private Color moonColor;
    [SerializeField] private float moonMaxIntensity;

    [Header("Night Ambient")]
    [SerializeField] private Color nightAmbient;

    private float timeRate;

    void Start()
    {
        timeRate = 24f / (dayLengthMinutes * 60f);
    }

    void Update()
    {
        if (preset == null)
            return;

        timeOfDay += Time.deltaTime * timeRate;
        timeOfDay %= 24;

        UpdateLighting(timeOfDay / 24f);
    }

    void UpdateLighting(float timePercent)
    {
        float sunAmount = Mathf.Clamp01(Mathf.Sin(timePercent * Mathf.PI));
        float moonAmount = 1f - sunAmount;

        Color dayAmbient = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, sunAmount);

        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);

        if (sun != null)
        {
            sun.color = preset.DirectionalColor.Evaluate(timePercent);
            sun.intensity = preset.LightIntensity.Evaluate(timePercent) * sunAmount;

            sun.transform.rotation =
                Quaternion.Euler((timePercent * 360f) - 90f, 170f, 0);
        }

        if (moon != null)
        {
            moon.color = moonColor;
            moon.intensity = moonAmount * moonMaxIntensity;

            moon.transform.rotation =
                Quaternion.Euler((timePercent * 360f) + 90f, 170f, 0);
        }
    }
}