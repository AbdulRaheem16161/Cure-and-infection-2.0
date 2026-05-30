using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    [SerializeField] private string bootstrapScene;
    [SerializeField] private string uiScene;
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string gameScene;

    public Task LoadBootstrap() => LoadSceneAsync(bootstrapScene, LoadSceneMode.Single);
    public Task LoadUI() => LoadSceneAsync(uiScene);
    public Task LoadMainMenu() => LoadSceneAsync(mainMenuScene);
    public Task LoadGame() => LoadSceneAsync(gameScene);

    public async Task LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

        if (operation == null)
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            return;
        }

        while (!operation.isDone)
            await Task.Yield();
    }

    public async Task UnloadSceneAsync(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            return;

        AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneName);

        if (operation == null)
        {
            Debug.LogError($"Failed to unload scene: {sceneName}");
            return;
        }

        while (!operation.isDone)
            await Task.Yield();
    }

    public bool IsLoaded(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName).isLoaded;
    }

    public Scene GetScene(string sceneName)
    {
        return SceneManager.GetSceneByName(sceneName);
    }

    public Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }

    public void SetActiveScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.isLoaded)
        {
            Debug.LogWarning(
                $"Cannot set active scene. '{sceneName}' is not loaded.");
            return;
        }

        SceneManager.SetActiveScene(scene);
    }
}
