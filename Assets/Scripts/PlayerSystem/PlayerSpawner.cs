using UnityEditor.PackageManager;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner Instance;
    public EntityDefinition playerDefinition;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Spawn player, has optional spawn point (reloading game with player keeping save pos), + built in backup
    /// </summary>
    public static PlayerController SpawnPlayer(Vector3? spawnPosition)
    {
        if (Instance == null)
        {
            Debug.LogError("PlayerSpawner Instance is null.");
            return null;
        }

        if (!spawnPosition.HasValue)
        {
            PlayerSpawnPositions playerSpawnPositions = FindAnyObjectByType<PlayerSpawnPositions>();
            if (playerSpawnPositions == null)
            {
                spawnPosition = TryGetSpawnPositionOnTerrain();
                Debug.LogError($"Failed to find " +
                    $"{typeof(PlayerSpawnPositions)} component in game scene: {GameManager.Instance.SceneHandler.GetActiveScene().name}");
            }
            else
                spawnPosition = playerSpawnPositions.GetRandomSpawnPosition();
        }

        return Instantiate(Instance.playerDefinition.gameObjectPrefab, (Vector3)spawnPosition, Quaternion.identity).GetComponent<PlayerController>();
    }

    /// <summary>
    /// should be replaced with method to get terrain ref of scene, then find random pos on terrain
    /// </summary>
    private static Vector3 TryGetSpawnPositionOnTerrain()
    {
        Vector3 spawnPosition = new(0, 1.5f, 0);

        if (Physics.Raycast(new Vector3(0, 1000, 0), Vector3.down, out RaycastHit hit, 10000))
            spawnPosition = new Vector3(spawnPosition.x, spawnPosition.y + 1.5f, spawnPosition.z);
        else
            Debug.LogError("Failed to find backup spawn location on terrain, defaulting to");

        return spawnPosition;
    }
}
