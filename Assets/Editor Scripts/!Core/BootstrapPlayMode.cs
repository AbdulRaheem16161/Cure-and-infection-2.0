using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class BootstrapPlayMode
{
    private const string bootstrapScenePath = "Assets/Scenes/BootstrapScene.unity";
    private const string key = "BOOTSTRAP_SCENE_TO_RESTORE";

    static BootstrapPlayMode()
    {
        EditorApplication.playModeStateChanged += OnStateChanged;
    }

    private static void OnStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var scenePath = SceneManager.GetActiveScene().path;
            Debug.LogError($"saved scene: {scenePath}");

            SessionState.SetString(key, scenePath);
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapScenePath);
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            EditorApplication.delayCall += LoadOriginalScene;
        }
    }

    private static void LoadOriginalScene()
    {
        var sceneToRestore = SessionState.GetString(key, "");
        Debug.LogError($"restored scene: {sceneToRestore}");

        if (string.IsNullOrEmpty(sceneToRestore))
            return;

        EditorSceneManager.LoadSceneAsyncInPlayMode(sceneToRestore, new LoadSceneParameters(LoadSceneMode.Additive));
    }
}