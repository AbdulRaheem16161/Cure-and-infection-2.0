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
                spawnPosition = new Vector3(0, 1.5f, 0); //needs better backup, probabaly downwards raycast to ground at 0,0
                Debug.LogError($"Failed to find " +
                    $"{typeof(PlayerSpawnPositions)} component in game scene: {GameManager.Instance.SceneHandler.GetActiveScene().name}");
            }
            else
                spawnPosition = playerSpawnPositions.GetRandomSpawnPosition();
        }

        return Instantiate(Instance.playerDefinition.gameObjectPrefab, (Vector3)spawnPosition, Quaternion.identity).GetComponent<PlayerController>();
    }
}
