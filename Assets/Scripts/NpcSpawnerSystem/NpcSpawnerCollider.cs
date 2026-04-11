using UnityEngine;

public class NpcSpawnerCollider : MonoBehaviour
{
	private NPCSpawner NPCSpawner;
	private PlayerController cachedPlayer;

	public SpawnerColliderType spawnerColliderType;
	public enum SpawnerColliderType { spawnValidZone, spawnBlockedZone}

	private void Awake()
	{
		NPCSpawner = GetComponentInParent<NPCSpawner>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (cachedPlayer == null)
			cachedPlayer = other.GetComponent<PlayerController>();

		if (cachedPlayer == null || other.GetComponent<PlayerController>() == null) return;

		switch (spawnerColliderType)
		{
			case SpawnerColliderType.spawnValidZone:
			NPCSpawner.playerInValidSpawnZone = true;
			break;

			case SpawnerColliderType.spawnBlockedZone:
			NPCSpawner.playerInBlockSpawnZone = true;
			break;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (cachedPlayer == null)
			cachedPlayer = other.GetComponent<PlayerController>();

		if (cachedPlayer == null || other.GetComponent<PlayerController>() == null) return;

		switch (spawnerColliderType)
		{
			case SpawnerColliderType.spawnValidZone:
			NPCSpawner.playerInValidSpawnZone = false;
			NPCSpawner.CleanUpAllNpcs(); //despawn when player too far
			break;

			case SpawnerColliderType.spawnBlockedZone:
			NPCSpawner.playerInBlockSpawnZone = false;
			break;
		}
	}
}
