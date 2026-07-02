/// <summary>
/// This script spawns an npc based on assigned NpcDefinition in the inspector using its linked prefab gameobject
/// sets its parent to this (NPCSpawner) and its position to (0, 0, 0),
/// it also instantiates 1 patrol, 1 random follow point and 1 spawn point along with the NPC
/// and sets their parent and position same as the NPC.
/// Lastly, it assigns the instantiated patrol, random and spawn points to the spawned NPC.NPCStateMachine
/// </summary>

/// <summary> TODO:
/// static npcs would benefit from more complex spawning/clean up logic. atm them working same as regular active npc that just despawn at further ranges is fine.
/// </summary>

using Game.MyNPC;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static Game.MyNPC.NPCStateMachine;

public class NPCSpawner : MonoBehaviour
{
	[Header("Spawner Sub Component Prefabs")]
	public GameObject patrolPathPrefab;
	public GameObject spawnPointPrefab;

	[Header("Npc Definition Lists")]
	public List<EntityDefinition> survivorDefinitions = new();
	public List<EntityDefinition> zombieDefinitions = new();

	[Header("Spawner Child Objects")]
	public GameObject movementAreaAndPathsParent;
	public GameObject spawnPointsParent;
	public GameObject activeNpcsParent;
	public GameObject inactiveNpcsParent;

	[Header("Patrol Paths, supports multiple")]
	public List<PatrolPathManager> PatrolPaths = new();

	[Header("Area Move Manager")]
	public RandomAreaMoveManager RandomAreaMoveManager;

	[Header("Toggle Gizmos")]
	public bool HideSpawnerRangeSphere;
    public bool HideAreaPoints;
	public bool HideAllPatrolPaths;
	public bool HideAllSpawnPoints;

	[Header("Spawner Settings")]
	[Tooltip("Forces custom npcs to be spawned in, ignoring max spawn distance check")]
    public bool forceSpawnCustomNpcs;
    [Tooltip("Keeps custom spawned Npc spawned in, ignoring distance clean up checks")]
    public bool keepCustomNpcsSpawned;
	public bool disableRandomSpawns;
	[Range(50, 1000)]
	public int spawnerRadius = 100;

    //values use squared distance to avoid sqrt calculations
    private readonly int minSqrSpawnDistanceFromPlayer = 50 * 50;
    private readonly int maxSqrSpawnDistanceFromPlayer = 100 * 100;
    private readonly int sqrDespawnDistanceFromPlayer = 160 * 160;
    private int sqrStaticDespawnDistanceFromPlayer;

	//spawner timers
    private const float NearbySpawnInterval = 3f;
    private const float DistantSpawnInterval = 10f;
    private const float CleanUpInterval = 25f;

    private float spawnTimer;
    private float cleanUpTimer;

    [Header("Npcs To Spawn")]
	public List<NpcSpawnData> CustomNpcsToSpawn = new();

	[Header("Random Npcs To Spawn")]
	public List<EntityDefinition> NpcsToRandomSpawn = new();
	public int minSpawnAmount;
	public int maxSpawnAmount;

	[Header("Runtime Info")]
	private PlayerController Player => GameManager.Instance.PlayerReference;
    public float playerSqrDistanceToSpawner;
    [SerializeField] private List<SpawnedNpcData> activeNpcs = new();
	[SerializeField] private List<SpawnedNpcData> inactiveNpcs = new();

	public enum Teams { Zombie, Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
	private readonly System.Random systemRandom = new();

	private void Awake()
	{
		StatsHandler.OnZombificationComplete += HandleNpcZombification;
        sqrStaticDespawnDistanceFromPlayer = spawnerRadius * spawnerRadius; //squared distance

        if (minSpawnAmount > maxSpawnAmount)
		{
			Debug.LogWarning($"{this} Npc Spawners minSpawnAmount bigger then maxSpawnAmount, equilizing");
			minSpawnAmount = maxSpawnAmount;
		}
		else if (maxSpawnAmount < minSpawnAmount)
		{
			Debug.LogWarning($"{this} Npc Spawners maxSpawnAmount smaller then minSpawnAmount, equilizing");
			maxSpawnAmount = minSpawnAmount;
		}

        float min = 8.0f;
        float max = 9.0f;
        spawnTimer = min + (float)systemRandom.NextDouble() * (max - min); //randomize first spawn timer to avoid all spawners spawning at same time
    }
    private void Update()
    {
		HandleNpcSpawning();
        HandleMpcCleanup();
    }
    private void OnDestroy()
	{
		StatsHandler.OnZombificationComplete -= HandleNpcZombification;
	}

    #region Npc Spawning
	private void HandleNpcSpawning()
	{
        playerSqrDistanceToSpawner = Player != null ? (Player.transform.position - transform.position).sqrMagnitude : Mathf.Infinity;
        float interval = playerSqrDistanceToSpawner < sqrStaticDespawnDistanceFromPlayer ? NearbySpawnInterval : DistantSpawnInterval;

        spawnTimer += Time.deltaTime;

        if (spawnTimer < interval)
            return;

        spawnTimer = 0f;

		if (ShouldSpawnCustomNpcs())
		{
			Debug.LogError("spawn custom npcs");
            SpawnCustomNpcs();
        }
		else
		{
            Debug.LogError("dont spawn custom npcs");
        }

		if (ShouldSpawnRandomNpcs())
            SpawnNpc(new(AssignRandomNpc(NpcsToRandomSpawn), MovementType.randomAreaMove, AssignRandomSpawnPointAroundPlayer()), false);
    }
	private bool ShouldSpawnRandomNpcs()
	{
		if (disableRandomSpawns) return false;
		return playerSqrDistanceToSpawner < sqrStaticDespawnDistanceFromPlayer;
    }
	private bool ShouldSpawnCustomNpcs()
    {
        if (forceSpawnCustomNpcs) return true;
        return playerSqrDistanceToSpawner < sqrStaticDespawnDistanceFromPlayer;
    }

    public void SpawnCustomNpcs()
	{
		int id = 0;

		foreach (NpcSpawnData npcSpawnData in CustomNpcsToSpawn)
		{
			if (npcSpawnData.Definition == null)
			{ Debug.LogError($"npcSpawnData.npcDefinition is null, skipping, assign one in inspector"); continue; }

            if (CustomNpcAlreadySpawned(npcSpawnData))
                continue;

            PatrolPathManager patrolPath = npcSpawnData.patrolPath;
			if (npcSpawnData.movementType == MovementType.patrolMove && patrolPath == null)
				npcSpawnData.patrolPath = AssignRandomPatrolPath();

			if (npcSpawnData.spawnPoint == null)
				npcSpawnData.spawnPoint = transform;

            float squaredDistance = (GameManager.Instance.PlayerReference.transform.position - npcSpawnData.spawnPoint.transform.position).sqrMagnitude;

			if (!forceSpawnCustomNpcs)
                if (squaredDistance < minSqrSpawnDistanceFromPlayer || squaredDistance > maxSqrSpawnDistanceFromPlayer) continue;

            npcSpawnData.SetId(id);
            SpawnNpc(npcSpawnData, true);
			id++;
		}
	}
	private bool CustomNpcAlreadySpawned(NpcSpawnData npcSpawnData)
    {
        foreach (SpawnedNpcData spawnedNpc in activeNpcs)
        {
            if (spawnedNpc.npcSpawnData.GetId() == npcSpawnData.GetId())
            {
                return true;
            }
        }
		return false;
    }
    #endregion

    #region Spawn Npcs
    private void SpawnNpc(NpcSpawnData npcSpawnData, bool staticSpawnedNpc)
	{
		if (npcSpawnData.Definition == null)
		{
			Debug.LogError("NpcDefinition null in spawn data assign a reference");
			return;
		}

		NpcController npcController = GetNpc(npcSpawnData.Definition);
		NPCStateMachine stateMachine = npcController.StateMachine;
		npcController.transform.SetParent(activeNpcsParent.transform);

		if (npcSpawnData.spawnPoint != null)
			npcController.transform.position = npcSpawnData.spawnPoint.transform.position;
		else
			npcController.transform.position = npcSpawnData.SpawnPosition;

		if (npcSpawnData.movementType == MovementType.patrolMove)
		{
			if (npcSpawnData.patrolPath != null)
				stateMachine.SetMovementType(npcSpawnData.movementType, npcSpawnData.patrolPath);
			else
				stateMachine.SetMovementType(MovementType.randomMove); //fall back to random move
		}

		else if (npcSpawnData.movementType == MovementType.randomAreaMove)
		{
			if (RandomAreaMoveManager != null)
				stateMachine.SetMovementType(npcSpawnData.movementType, RandomAreaMoveManager);
			else
				stateMachine.SetMovementType(MovementType.randomMove); //fall back to random move
		}

		else if (npcSpawnData.movementType == MovementType.randomMove)
			stateMachine.SetMovementType(npcSpawnData.movementType);

		npcController.InitializeNpc(npcSpawnData.Definition, npcSpawnData.team);
		activeNpcs.Add(new(npcController, staticSpawnedNpc, GameManager.Instance.PlayerReference, npcSpawnData));

		if (npcSpawnData.forceInvincible)
			npcController.StatsHandler.invincible = true;
		if (npcSpawnData.forceDeath)
			npcController.StatsHandler.DebugKillNpc();
	}
    #endregion

    #region Npc Spawner Helpers
    private EntityDefinition AssignRandomNpc(List<EntityDefinition> npcDefinitions)
	{
		if (npcDefinitions.Count == 0) return null;
		return npcDefinitions[systemRandom.Next(0, npcDefinitions.Count)];
	}
    private PatrolPathManager AssignRandomPatrolPath()
	{
		if (PatrolPaths.Count == 0)
        { Debug.LogWarning($"No patrol paths assigned, NPCs will use fallback movement."); return null; }
		return PatrolPaths[systemRandom.Next(0, PatrolPaths.Count)];
	}
    private Vector3 AssignRandomSpawnPointAroundPlayer()
    {
        Vector3 playerPos = GameManager.Instance.PlayerReference.transform.position;

        const int maxAttempts = 20;

        for (int i = 0; i < maxAttempts; i++)
        {
            float min = minSqrSpawnDistanceFromPlayer;
            float max = maxSqrSpawnDistanceFromPlayer;

            double angle = systemRandom.NextDouble() * (Math.PI * 2.0);
            float distance = Mathf.Sqrt((float)systemRandom.NextDouble() * (max * max - min * min) + min * min);
            Vector3 point = playerPos + new Vector3(Mathf.Cos((float)angle), 0f, Mathf.Sin((float)angle)) * distance;

            if (NavMesh.SamplePosition(point, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return playerPos;
    }
    #endregion

    #region Npc Clean Up
    private void HandleMpcCleanup()
    {
        cleanUpTimer += Time.deltaTime;

        if (cleanUpTimer < CleanUpInterval)
            return;

        cleanUpTimer = 0f;
        CleanUpNpcs();
    }
    public void CleanUpNpcs()
    {
        for (int i = activeNpcs.Count - 1; i >= 0; i--)
        {
            SpawnedNpcData spawnedNpcData = activeNpcs[i];

            if (spawnedNpcData.staticSpawnedNpc && keepCustomNpcsSpawned) continue;

            spawnedNpcData.UpdateSquaredDistanceFromPlayer(GameManager.Instance.PlayerReference);
            float squaredDistanceToCleanUp = spawnedNpcData.staticSpawnedNpc ? sqrStaticDespawnDistanceFromPlayer : sqrDespawnDistanceFromPlayer;

            if (spawnedNpcData.squaredDistanceToPlayer > squaredDistanceToCleanUp)
                CleanUpNpc(spawnedNpcData);
        }
    }
    public void CleanUpDeadNpcs()
    {
        for (int i = activeNpcs.Count - 1; i >= 0; i--)
        {
            if (activeNpcs[i].npc.StatsHandler.LifeState != EntityDefinition.LifeState.dead) continue;
            CleanUpNpc(activeNpcs[i]);
        }
    }
    private void CleanUpNpc(SpawnedNpcData spawnedNpc)
    {
        inactiveNpcs.Add(spawnedNpc);
        activeNpcs.Remove(spawnedNpc);
        spawnedNpc.npc.gameObject.SetActive(false);
        spawnedNpc.npc.transform.SetParent(inactiveNpcsParent.transform);
    }
    #endregion

    #region Fetch Inactive Npcs Or Create New From Pooling
    private NpcController GetNpc(EntityDefinition npcDefinition)
    {
		NpcController npc = null;

		for (int i = inactiveNpcs.Count - 1; i >= 0; i--)
		{
			if (inactiveNpcs[i].npc.Definition.StartingLifeState != npcDefinition.StartingLifeState) continue;

			npc = inactiveNpcs[i].npc;
			inactiveNpcs.RemoveAt(i);
			break;
		}

		if (npc == null)
			npc = Instantiate(npcDefinition.gameObjectPrefab, transform.position, Quaternion.identity).GetComponent<NpcController>();

		npc.transform.SetParent(transform);
		npc.transform.localPosition = Vector3.zero;
		npc.gameObject.SetActive(true);
		return npc;
	}

    #endregion

	#region handle zombification events (TODO will need updating to only listen for npc it spawned)
	private void HandleNpcZombification(GameObject gameObject)
	{
		foreach (SpawnedNpcData spawnedNpc in activeNpcs)
        {
            if (spawnedNpc.npc.gameObject == gameObject)
			{
                CleanUpNpc(spawnedNpc);
                SpawnNpc(new(AssignRandomNpc(zombieDefinitions), MovementType.randomAreaMove, gameObject.transform.position), false);
				return;
            }
        };
	}
	#endregion

    //editor methods
    #region Create New Paths and Spawn Points From Editor
    public void CreateNewPatrolPointPath()
	{
		GameObject patrolPath = (GameObject)PrefabUtility.InstantiatePrefab(patrolPathPrefab);
		patrolPath.transform.SetParent(movementAreaAndPathsParent.transform);
		patrolPath.transform.position = transform.position;
		PatrolPathManager patrolPathManager = patrolPath.GetComponent<PatrolPathManager>();
		PatrolPaths.Add(patrolPathManager);
		patrolPath.name += $"{movementAreaAndPathsParent.transform.childCount - 1}"; //-1 due to area move manager
		Selection.activeGameObject = patrolPath;
	}
	public void CreateNewSpawnPoint()
	{
		GameObject spawnPoint = (GameObject)PrefabUtility.InstantiatePrefab(spawnPointPrefab);
		spawnPoint.transform.SetParent(spawnPointsParent.transform);
		spawnPoint.transform.position = transform.position;
		spawnPoint.name += $"{spawnPointsParent.transform.childCount}";
		Selection.activeGameObject = spawnPoint;
	}
	#endregion

	#region Spawn Random Or Specified Npcs From Editor
	public void SpawnRandomSurvivorNpc(Teams team, MovementType movementType)
	{
		SpawnNpc(new(AssignRandomNpc(survivorDefinitions), team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPointAroundPlayer()), false);
	}
	public void SpawnRandomZombieNpc(Teams team, MovementType movementType)
	{
		SpawnNpc(new(AssignRandomNpc(zombieDefinitions), team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPointAroundPlayer()), false);
	}
	public void SpawnSpecifiedNpc(EntityDefinition npcDefinition, Teams team, MovementType movementType)
	{
		SpawnNpc(new(npcDefinition, team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPointAroundPlayer()), false);
	}
	#endregion

	private void OnDrawGizmos()
	{
		if (!HideSpawnerRangeSphere)
		{
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnerRadius);
        }

		if (!HideAreaPoints)
			RandomAreaMoveManager.DrawAreaPointsAndTriangles();

		if (!HideAllPatrolPaths)
		{
			foreach (PatrolPathManager patrolPath in PatrolPaths)
				patrolPath.DrawPatrolPathPoints();
		}

		if (!HideAllSpawnPoints)
		{
			for (int i = 0; i < spawnPointsParent.transform.childCount; i++)
			{
				Gizmos.color = new(1f, 0.85f, 0.1f); // warm amber
				Gizmos.DrawSphere(spawnPointsParent.transform.GetChild(i).transform.position, 1f); //draw a Sphere on every point
			}
		}
	}
}
