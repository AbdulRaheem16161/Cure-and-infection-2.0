using UnityEngine;

public class RandomMaterialAssigner : MonoBehaviour
{
    [Tooltip("Name of the shader to match before applying color")]
    private const string TargetShaderName = "Shader Graphs/ToonShader";

    void Awake()
    {
        Color randomColor = GenerateRandomColor();

        foreach (Transform child in transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();

            if (renderer != null && renderer.sharedMaterial != null &&
                renderer.sharedMaterial.shader != null &&
                renderer.sharedMaterial.shader.name == TargetShaderName)
            {
                // Clone the material to avoid editing the shared asset if needed
                Material clonedMaterial = new Material(renderer.sharedMaterial);
                clonedMaterial.color = randomColor;
                renderer.material = clonedMaterial; // Apply instance so only this child uses it
            }
        }
    }

    private Color GenerateRandomColor()
    {
        // Hue (0-1), Saturation (0.6-1), Value (0.6-1) = decent colors
        return Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
    }
}
