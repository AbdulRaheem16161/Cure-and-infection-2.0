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
    [SerializeField] private float dayLengthMinutes;

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
        RenderSettings.ambientLight = preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = preset.FogColor.Evaluate(timePercent);

        if (sun != null)
        {
            sun.color = preset.DirectionalColor.Evaluate(timePercent);
            sun.intensity = preset.LightIntensity.Evaluate(timePercent);

            sun.transform.rotation =
                Quaternion.Euler((timePercent * 360f) - 90f, 170f, 0);
        }

        if (moon != null)
        {
            moon.intensity = 1 - preset.LightIntensity.Evaluate(timePercent);

            moon.transform.rotation =
                Quaternion.Euler((timePercent * 360f) + 90f, 170f, 0);
        }
    }
}