using UnityEngine;

[CreateAssetMenu(menuName="Lighting/LightingPreset")]
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;

    public AnimationCurve LightIntensity;
}
