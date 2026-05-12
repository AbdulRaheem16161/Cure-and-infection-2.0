/// <summary>
/// This script spawns an npc based on assigned NpcDefinition in the inspector using its linked prefab gameobject
/// sets its parent to this (NPCSpawner) and its position to (0, 0, 0),
/// it also instantiates 1 patrol, 1 random follow point and 1 spawn point along with the NPC
/// and sets their parent and position same as the NPC.
/// Lastly, it assigns the instantiated patrol, random and spawn points to the spawned NPC.NPCStateMachine
/// </summary>

/// <summary> TODO:
/// set triggers to spawn enemies when player gets close enough + despawn when they get too far away (use object pooling)
/// set triggers to stop the spawning of enemies when player is too close (stops things spawning infront of player)
/// add toggle to set spawning to center of spawner or be random within npcs wander area.
/// 
/// this works extremely well for making patrol paths and managing enemies in map sections. but needs more customization eg:
/// list of NpcDefinitions to spawn, weather or not they will spawn with patrols paths or random paths with a bool. scenario one example:
/// spawner at center of a survivor camp, assign a list of 10 npcs. 
/// 5 of those toggled to use 1 of 3 random patrol paths (possible option to specify a patrol path)
/// the other 5 will randomly wander within a the given area.
/// 
/// spawner in the wilderness/edge of town: these spawners can have some random wander area overlap with others and have larger areas
/// limit of spawning 20 over the area. all set to random spawn in the area + randomly wander within the area.
/// 
/// spawner in towns cities: these spawners can have some random wander area overlap with others, have smaller areas
/// esentially same as spawner above
/// </summary>

using Game.MyNPC;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Game.MyNPC.NPCStateMachine;
using static NPCSpawner;

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
	public bool ShowAreaPoints;
	public bool ShowAllPatrolPaths;
	public bool ShowAllSpawnPoints;

	[Header("Spawner Settings")]
	[Tooltip("bypasses checks that keep npcs spawned around player, and despawning when player is far away")]
	public bool forceSpawnNpcs;
	public SpawnerType spawnerType;
	public enum SpawnerType
	{
		random, custom, both
	}
	public bool playerInValidSpawnZone;
	public bool playerInBlockSpawnZone;
	private bool SpawnNpcs => ShouldSpawnNpcs();
	private bool respawnCustomNpcs;

	[Header("Npcs To Spawn")]
	public List<NpcSpawnData> CustomNpcsToSpawn = new();

	[Header("Random Npcs To Spawn")]
	private readonly float spawnTimerCooldown = 1f;
	private float spawnTimer;
	public List<EntityDefinition> NpcsToRandomSpawn = new();
	public int minSpawnAmount;
	public int maxSpawnAmount;

	[Header("Runtime Info")]
	[SerializeField] private List<NpcController> activeNpcs = new();
	[SerializeField] private List<NpcController> inactiveNpcs = new();

	public enum Teams { Zombie, Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
	private readonly System.Random systemRandom = new();

	private void Awake()
	{
		respawnCustomNpcs = true;
		StatsHandler.OnZombificationComplete += HandleNpcZombification;

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
	}
	private void OnDestroy()
	{
		StatsHandler.OnZombificationComplete -= HandleNpcZombification;
	}

	private void Update()
	{
		if (!SpawnNpcs) return;
		if (spawnerType == SpawnerType.random || spawnerType == SpawnerType.both)
			SpawnRandomNpcs();
		if (spawnerType == SpawnerType.custom || spawnerType == SpawnerType.both)
		{
			if (respawnCustomNpcs)
				SpawnCustomNpcs();
		}
	}

	#region spawn npcs on start and update
	private void SpawnRandomNpcs()
	{
		spawnTimer -= Time.deltaTime;
		if (spawnTimer > 0) return;
		spawnTimer = spawnTimerCooldown;

		while (activeNpcs.Count < maxSpawnAmount)
			SpawnRandomNpc();
	}
	public void SpawnCustomNpcs()
	{
		respawnCustomNpcs = false;
		foreach (NpcSpawnData npcSpawnData in CustomNpcsToSpawn)
		{
			if (npcSpawnData.Definition == null)
			{ Debug.LogError($"npcSpawnData.npcDefinition is null, skipping, assign one in inspector"); continue; }

			PatrolPathManager patrolPath = npcSpawnData.patrolPath;
			if (npcSpawnData.movementType == MovementType.patrolMove && patrolPath == null)
				npcSpawnData.patrolPath = AssignRandomPatrolPath();

			if (npcSpawnData.spawnPoint == null)
				npcSpawnData.spawnPoint = transform;

			SpawnNpc(npcSpawnData);
		}
	}
	#endregion

	#region spawn npcs
	private void SpawnRandomNpc()
	{
		NpcSpawnData spawnData = new(AssignRandomNpc(NpcsToRandomSpawn), MovementType.randomAreaMove, AssignRandomSpawnPosition());

		if (!spawnData.Definition.Flags.HasFlag(EntityDefinition.EntityFlags.canBecomeZombie)) //cant become zombie so is zombie
			spawnData.team = Teams.Zombie;

		SpawnNpc(spawnData); //path null as randoms spawns dont have paths for now
	}
	private void SpawnNpc(NpcSpawnData npcSpawnData)
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
		activeNpcs.Add(npcController);

		if (npcSpawnData.forceInvincible)
			npcController.StatsHandler.invincible = true;
		if (npcSpawnData.forceDeath)
			npcController.StatsHandler.DebugKillNpc();
	}
	#endregion

	#region npc spawner helpers
	public EntityDefinition AssignRandomNpc(List<EntityDefinition> npcDefinitions)
	{
		if (npcDefinitions.Count == 0)
		{ Debug.LogError("npcDefinitions.Count == 0, assign definition references to list"); return null; }
		return npcDefinitions[systemRandom.Next(0, npcDefinitions.Count)];
	}
	public PatrolPathManager AssignRandomPatrolPath()
	{
		if (PatrolPaths.Count == 0)
		{ Debug.LogError("PatrolPaths.Count == 0, assign patrol path references"); return null; }
		return PatrolPaths[systemRandom.Next(0, PatrolPaths.Count)];
	}
	private Vector3 AssignRandomSpawnPosition()
	{
		return Vector3.zero;
	}
	#endregion

	#region fetching valid npc from pooling, or instantiating new one
	private NpcController GetNpc(EntityDefinition npcDefinition)
    {
		NpcController npc = null;

		for (int i = inactiveNpcs.Count - 1; i >= 0; i--)
		{
			if (inactiveNpcs[i].Definition.StartingLifeState != npcDefinition.StartingLifeState) continue;

			npc = inactiveNpcs[i];
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

	#region npc clean up and repooling
	public void CleanUpAllNpcs(bool resetRespawnCustomNpcsBool = true)
	{
		for (int i = activeNpcs.Count - 1; i >= 0; i--)
			CleanUpNpc(activeNpcs[i]);

		if (resetRespawnCustomNpcsBool)
			respawnCustomNpcs = true;
	}
	public void CleanUpDeadNpcs()
	{
		for (int i = activeNpcs.Count - 1; i >= 0; i--)
		{
			if (activeNpcs[i].StatsHandler.LifeState != EntityDefinition.LifeState.dead) continue;
			CleanUpNpc(activeNpcs[i]);
		}
	}
	private void CleanUpNpc(NpcController npcController)
	{
		inactiveNpcs.Add(npcController);
		activeNpcs.Remove(npcController);
		npcController.gameObject.SetActive(false);
		npcController.transform.SetParent(inactiveNpcsParent.transform);
	}
	#endregion

	#region handle zombification events (TODO will need updating to only listen for npc it spawned)
	private void HandleNpcZombification(GameObject gameObject)
	{
		CleanUpNpc(gameObject.GetComponent<NpcController>());
		SpawnNpc(new(AssignRandomNpc(zombieDefinitions), MovementType.randomAreaMove, gameObject.transform.position));
	}
	#endregion

	#region
	private bool ShouldSpawnNpcs()
	{
		if (forceSpawnNpcs)
			return true;

		bool result = playerInValidSpawnZone && !playerInBlockSpawnZone;
		Debug.LogError(result);
		return result;
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
		SpawnNpc(new(AssignRandomNpc(survivorDefinitions), team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPosition()));
	}
	public void SpawnRandomZombieNpc(Teams team, MovementType movementType)
	{
		SpawnNpc(new(AssignRandomNpc(zombieDefinitions), team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPosition()));
	}
	public void SpawnSpecifiedNpc(EntityDefinition npcDefinition, Teams team, MovementType movementType)
	{
		SpawnNpc(new(npcDefinition, team, movementType, AssignRandomPatrolPath(), AssignRandomSpawnPosition()));
	}
	#endregion

	private void OnDrawGizmos()
	{
		if (ShowAreaPoints)
			RandomAreaMoveManager.DrawAreaPointsAndTriangles();

		if (ShowAllPatrolPaths)
		{
			foreach (PatrolPathManager patrolPath in PatrolPaths)
				patrolPath.DrawPatrolPathPoints();
		}

		if (ShowAllSpawnPoints)
		{
			for (int i = 0; i < spawnPointsParent.transform.childCount; i++)
			{
				Gizmos.color = new(1f, 0.85f, 0.1f); // warm amber
				Gizmos.DrawSphere(spawnPointsParent.transform.GetChild(i).transform.position, 1f); //draw a Sphere on every point
			}
		}
	}
}
