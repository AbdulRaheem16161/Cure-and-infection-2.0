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

public class NPCSpawner : MonoBehaviour
{
    #region References
    public GameObject TRex;
    public GameObject Fighter;
    public GameObject Zombie;
    public GameObject RandomFollowPoint;
    public GameObject PatrolFollowPoint;
    public GameObject SpawnPoint;

    public TrackGizmos TrackGizmos;
	#endregion

	[SerializeField] private List<NpcController> npcObjectPooling = new();

	#region Enums
	public enum Teams { Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
    public Teams NPCsTeam;

    public NpcDefinition npcDefinitionToSpawn;

	#endregion

	private void OnEnable()
	{
        NPCStateMachine.OnDeathComplete += HandleNpcDeath;
	}
	private void OnDisable()
	{
		NPCStateMachine.OnDeathComplete -= HandleNpcDeath;
	}

	public void SpawnNPC(NpcDefinition npcDefinition)
    {
        if (npcDefinition == null)
        {
            Debug.LogError("NpcDefinition null assign a reference");
            return;
        }

		#region get new npc and its refs
		GameObject NPCInstance = GetNewNpc(npcDefinition);
		NpcController npcController = NPCInstance.GetComponent<NpcController>();
		NPCStateMachine stateMachine = npcController.StateMachine;
		#endregion

		#region assign new points if null or re-enable
		if (stateMachine.RandomFollowPoint == null)
            stateMachine.AssignFollowPoint(GenerateWaypoints(RandomFollowPoint));
		else
			stateMachine.RandomFollowPoint.SetActive(true);

		if (stateMachine.PatrolFollowPoint == null)
			stateMachine.AssignPatrolPoint(GenerateWaypoints(PatrolFollowPoint), TrackGizmos);
		else
			stateMachine.PatrolFollowPoint.SetActive(true);

        if (stateMachine.SpawnPoint == null)
            stateMachine.AssignSpawnPoint(GenerateWaypoints(SpawnPoint));
		else
			stateMachine.SpawnPoint.SetActive(true);
		#endregion

		npcController.InitializeNpc(npcDefinitionToSpawn, NPCsTeam);
	}

	private GameObject GenerateWaypoints(GameObject GameObjectToSpawn)
    {
        #region Instantiate and Parent
        GameObject Instance = Instantiate(GameObjectToSpawn, transform.position, Quaternion.identity);
        Instance.transform.SetParent(transform);
        Instance.transform.localPosition = Vector3.zero;
        return Instance;
        #endregion
    }

	#region get or instantiate new npc from pooling
	private GameObject GetNewNpc(NpcDefinition npcDefinition)
	{
		GameObject Instance = TryGetNpcObjectFromPooling(npcDefinition);
		Instance.transform.SetParent(transform);
		Instance.transform.localPosition = Vector3.zero;
		Instance.SetActive(true);
		return Instance;
	}

	private GameObject TryGetNpcObjectFromPooling(NpcDefinition npcDefinition)
    {
        foreach (NpcController npcController in npcObjectPooling)
        {
            if (npcController.NpcDefinition.IsZombie == npcDefinition.IsZombie)
                return npcController.gameObject;
		}
		return Instantiate(npcDefinition.gameObjectPrefab, transform.position, Quaternion.identity);
	}
	#endregion

	#region handle npc deaths and adding to object pooling
    private void HandleNpcDeath(GameObject gameObject)
    {
        gameObject.SetActive(false);
        NpcController npcController = gameObject.GetComponent<NpcController>();
        NPCStateMachine stateMachine = npcController.StateMachine;

		stateMachine.RandomFollowPoint.SetActive(false);
		stateMachine.PatrolFollowPoint.SetActive(false);
		stateMachine.SpawnPoint.SetActive(false);

		npcObjectPooling.Add(npcController);
    }
	#endregion
}
