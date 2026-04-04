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

using System;
using System.Collections.Generic;
using UnityEngine;
using Game.MyNPC;
using static NpcDefinition;

public class NPCSpawner : MonoBehaviour
{
    #region References
	[Header("References")]
	public TrackGizmos TrackGizmos;
	public RandomMovementManager RandomMovementManager;
	#endregion

	[Header("Npc Definition List")]
	public List<NpcDefinition> npcSpawns = new();

	[Header("Zombie Npc Definition List")]
	public List<NpcDefinition> zombieNpcSpawns = new();

	[Header("object pooling list")]
	[SerializeField] private List<NpcController> npcObjectPooling = new();

	#region Enums
	public enum Teams { Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
    [HideInInspector] public Teams NPCsTeam;
	#endregion

	[HideInInspector] public NpcDefinition npcDefinitionToSpawn;

	private readonly System.Random systemRandom = new();

	private void OnEnable()
	{
        NPCStateMachine.OnDeathComplete += HandleNpcDeath;
		StatsHandler.OnZombificationComplete += HandleNpcZombification;
	}
	private void OnDisable()
	{
		NPCStateMachine.OnDeathComplete -= HandleNpcDeath;
		StatsHandler.OnZombificationComplete -= HandleNpcZombification;
	}

	#region spawn npc methods
	public void SpawnNPC(NpcDefinition npcDefinition, Vector3? spawnPosition = null)
	{
		if (npcDefinition == null)
        {
            Debug.LogError("NpcDefinition null assign a reference");
            return;
        }

		GameObject NPCInstance = GetNpcObject(npcDefinition);
		NpcController npcController = NPCInstance.GetComponent<NpcController>();
		NPCStateMachine stateMachine = npcController.StateMachine;

		Vector3 position = spawnPosition ?? transform.position;
		NPCInstance.transform.position = position;

		if (stateMachine.PatrolPoints == null)
			stateMachine.AssignPatrolPoints(TrackGizmos);

		if (stateMachine.RandomMovementManager == null)
            stateMachine.AssignMoveArea(RandomMovementManager);

		npcController.InitializeNpc(npcDefinitionToSpawn, NPCsTeam);
	}
	public void SpawnRandomNPC(bool spawnZombie, Vector3? spawnPosition = null)
	{
		NpcDefinition npcDefinition;

		if (spawnZombie)
			npcDefinition = zombieNpcSpawns[systemRandom.Next(0, zombieNpcSpawns.Count + 1)];
		else
			npcDefinition = npcSpawns[systemRandom.Next(0, npcSpawns.Count + 1)];

		SpawnNPC(npcDefinition, spawnPosition);
	}
	#endregion

	#region npc object pooling via GetNpcObject and HandleNpcDeath event (TODO will need updating to only listen for npc it spawned)
	private GameObject GetNpcObject(NpcDefinition npcDefinition)
    {
		GameObject npcObject = null;

		for (int i = 0; i < npcObjectPooling.Count; i++)
		{
			var npcController = npcObjectPooling[i];

			if (npcController.NpcDefinition.StartingLifeState == npcDefinition.StartingLifeState)
			{
				npcObject = npcController.gameObject;
				npcObjectPooling.RemoveAt(i);
				break;
			}
		}

		if (npcObject == null)
			npcObject = Instantiate(npcDefinition.gameObjectPrefab, transform.position, Quaternion.identity);

		npcObject.transform.SetParent(transform);
		npcObject.transform.localPosition = Vector3.zero;
		npcObject.SetActive(true);
		return npcObject;
	}

	private void HandleNpcDeath(GameObject gameObject)
    {
		gameObject.SetActive(false);
        NpcController npcController = gameObject.GetComponent<NpcController>();
		npcObjectPooling.Add(npcController);
	}
	#endregion

	#region handle zombification events (TODO will need updating to only listen for npc it spawned)
	private void HandleNpcZombification(GameObject gameObject)
	{
		HandleNpcDeath(gameObject);
		SpawnRandomNPC(true, gameObject.transform.position);
	}
	#endregion
}
