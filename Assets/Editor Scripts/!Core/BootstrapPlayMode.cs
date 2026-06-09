using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BootstrapPlayMode
{
    private const string bootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
    private const string uiScenePath = "Assets/Scenes/UiScene.unity";
    private const string key = "BOOTSTRAP_SCENE_TO_RESTORE";
    public static string Key => key;

    static BootstrapPlayMode()
    {
        EditorApplication.playModeStateChanged += OnStateChanged;
    }

    private static void OnStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var scenePath = SceneManager.GetActiveScene().path;

            if (scenePath == bootstrapScenePath) //if play scene is bootstrap, dont do anything
            {
                SessionState.SetString(key, null);
                return;
            }

            SessionState.SetString(key, scenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapScenePath);
            Debug.LogWarning($"Not starting from boostrap scene, injecting it and reloading play scene: {scenePath}");

            if (scenePath == uiScenePath) //if play scene is ui scene, dont restore let boostrap load ui
            {
                SessionState.SetString(key, null);
                return;
            }
        }
    }
}