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
using UnityEngine;
using static Game.MyNPC.NPCStateMachine;
using static NPCSpawner;

public class NPCSpawner : MonoBehaviour
{
	[Header("Spawner Sub Component Prefabs")]
	public GameObject patrolPathPrefab;
	public GameObject spawnPointPrefab;

	[Header("Spawner Child Objects")]
	public GameObject movementAreaAndPathsParent;
	public GameObject spawnPointsParent;
	public GameObject activeNpcsParent;
	public GameObject inactiveNpcsParent;

	[Header("Patrol Paths, supports multiple")]
	public List<PatrolPathManager> PatrolPaths = new();

	[Header("Area Move Manager")]
	public RandomAreaMoveManager RandomAreaMoveManager;

	[Header("Npc Definition Lists")]
	public List<NpcDefinition> survivorDefinitions = new();
	public List<NpcDefinition> zombieDefinitions = new();

	[InspectorLabel("Spawner Settings")]
	public SpawnerType spawnerType;
	public enum SpawnerType
	{
		random, custom, both
	}

	[Header("Npcs To Spawn")]
	public List<NpcSpawnData> CustomNpcsToSpawn = new();

	[Header("Random Npcs To Spawn")]
	private readonly float spawnTimerCooldown = 1f;
	private float spawnTimer;
	public List<NpcDefinition> NpcsToRandomSpawn = new();
	public int minSpawnAmount;
	public int maxSpawnAmount;

	[Header("Runtime Info")]
	[SerializeField] private List<NpcController> activeNpcs = new();
	[SerializeField] private List<NpcController> inactiveNpcs = new();

	public enum Teams { Zombie, Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
	private readonly System.Random systemRandom = new();

	private void Awake()
	{
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

	private void Start()
	{
		if (spawnerType == SpawnerType.custom || spawnerType == SpawnerType.both)
			SpawnCustomNpcs();
	}
	private void Update()
	{
		if (spawnerType == SpawnerType.random || spawnerType == SpawnerType.both)
			SpawnRandomNpcs();
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
		foreach (NpcSpawnData npcSpawnData in CustomNpcsToSpawn)
		{
			if (npcSpawnData.npcDefinition == null)
			{ Debug.LogError($"npcSpawnData.npcDefinition is null, skipping, assign one in inspector"); continue; }

			PatrolPathManager patrolPath = npcSpawnData.patrolPath;
			if (npcSpawnData.movementType == MovementType.patrolMove && patrolPath == null)
				patrolPath = AssignRandomPatrolPath();

			Vector3 spawnPoint = Vector3.zero;

			if (npcSpawnData.spawnPoint != null)
				spawnPoint = npcSpawnData.spawnPoint.transform.position;

			SpawnNPC(npcSpawnData.npcDefinition, npcSpawnData.team, npcSpawnData.movementType, patrolPath, spawnPoint);
		}
	}
	#endregion

	#region spawn npcs
	private void SpawnRandomNpc()
	{
		NpcDefinition npcToSpawn = AssignRandomNpc();
		Teams randomTeam = AssignRandomTeam();
		Vector3 spawnPoint = AssignRandomSpawnPosition();

		if (npcToSpawn == null) return;

		if (!npcToSpawn.Flags.HasFlag(NpcDefinition.EntityFlags.canBecomeZombie)) //cant become zombie so is zombie
			randomTeam = Teams.Zombie;

		SpawnNPC(npcToSpawn, randomTeam, MovementType.randomAreaMove, null, spawnPoint); //path null as randoms spawns dont have paths for now
	}
	private void SpawnNPC(NpcDefinition npcDefinition, Teams team, 
		MovementType movementType, PatrolPathManager patrolPath, Vector3 spawnPosition)
	{
		if (npcDefinition == null)
        {
            Debug.LogError("NpcDefinition null assign a reference");
            return;
        }

		NpcController npcController = GetNpc(npcDefinition);
		NPCStateMachine stateMachine = npcController.StateMachine;
		npcController.transform.SetParent(activeNpcsParent.transform);
		npcController.transform.position = spawnPosition;

		if (movementType == MovementType.patrolMove)
		{
			if (patrolPath != null)
				stateMachine.SetMovementType(movementType, patrolPath);
			else
				stateMachine.SetMovementType(MovementType.randomMove); //fall back to random move
		}

		else if (movementType == MovementType.randomAreaMove)
		{ 
			if (RandomAreaMoveManager != null)
				stateMachine.SetMovementType(movementType, RandomAreaMoveManager);
			else
				stateMachine.SetMovementType(MovementType.randomMove); //fall back to random move
		}

		else if (movementType == MovementType.randomMove)
			stateMachine.SetMovementType(movementType);

		npcController.InitializeNpc(npcDefinition, team);
		activeNpcs.Add(npcController);
	}
	#endregion

	#region npc spawner helpers
	public NpcDefinition AssignRandomNpc()
	{
		if (NpcsToRandomSpawn.Count == 0)
		{ Debug.LogError("NpcsToRandomSpawn.Count == 0, assign definition refereces"); return null; }
		return NpcsToRandomSpawn[systemRandom.Next(0, NpcsToRandomSpawn.Count)];
	}
	public Teams AssignRandomTeam()
	{
		Array teams = Enum.GetValues(typeof(Teams));
		return(Teams)teams.GetValue(systemRandom.Next(teams.Length));
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
	private NpcController GetNpc(NpcDefinition npcDefinition)
    {
		NpcController npc = null;

		for (int i = 0; i < inactiveNpcs.Count; i++)
		{
			npc = inactiveNpcs[i];

			if (npc.NpcDefinition.StartingLifeState == npcDefinition.StartingLifeState)
			{
				inactiveNpcs.RemoveAt(i);
				break;
			}
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
	public void CleanUpAllNpcs()
	{
		for (int i = activeNpcs.Count - 1; i >= 0; i--)
			CleanUpNpc(activeNpcs[i]);
	}
	public void CleanUpDeadNpcs()
	{
		for (int i = activeNpcs.Count - 1; i >= 0; i--)
		{
			if (activeNpcs[i].StatsHandler.LifeState != NpcDefinition.LifeState.dead) continue;
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
		//SpawnRandomNPC(true, gameObject.transform.position);
	}
	#endregion

	#region debug editor spawner options
	public void SpawnRandomSurvivorNpc(Teams team, MovementType movementType)
	{
		NpcDefinition npcToSpawn = survivorDefinitions[systemRandom.Next(0, survivorDefinitions.Count)];
		PatrolPathManager patrolPathManager = AssignRandomPatrolPath();

		if (!npcToSpawn.Flags.HasFlag(NpcDefinition.EntityFlags.canBecomeZombie)) //cant become zombie so is zombie
			team = Teams.Zombie;

		SpawnNPC(npcToSpawn, team, movementType, patrolPathManager, Vector3.zero);
	}
	public void SpawnRandomZombieNpc(Teams team, MovementType movementType)
	{
		NpcDefinition npcToSpawn = zombieDefinitions[systemRandom.Next(0, zombieDefinitions.Count)];
		PatrolPathManager patrolPathManager = AssignRandomPatrolPath();

		if (!npcToSpawn.Flags.HasFlag(NpcDefinition.EntityFlags.canBecomeZombie)) //cant become zombie so is zombie
			team = Teams.Zombie;

		SpawnNPC(npcToSpawn, team, movementType, patrolPathManager, Vector3.zero);
	}
	public void SpawnSpecifiedNpc(NpcDefinition npcDefinition, Teams team, MovementType movementType)
	{
		PatrolPathManager patrolPathManager = AssignRandomPatrolPath();
		SpawnNPC(npcDefinition, team, movementType, patrolPathManager, Vector3.zero);
	}
	#endregion
}

[Serializable]
public class NpcSpawnData
{
	[Tooltip("Npc definition to spawn")]
	public NpcDefinition npcDefinition;

	[Tooltip("Spawned Npcs team")]
	public Teams team;

	[Tooltip("Spawned Npcs movement type. NPC handles fallback: Patrol > RandomArea > Random")]
	public MovementType movementType;

	[Tooltip("Spawned patrol path, null = randomly chosen")]
	public PatrolPathManager patrolPath;

	[Tooltip("Spawned Npcs spawn point, null = spawner location")]
	public Transform spawnPoint;
}
