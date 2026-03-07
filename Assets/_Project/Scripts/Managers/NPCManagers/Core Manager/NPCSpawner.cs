/// <summary>
/// This script spawns an npc based on assigned NpcDefinition in the inspector using its linked prefab gameobject
/// sets its parent to this (NPCSpawner) and its position to (0, 0, 0),
/// it also instantiates 1 patrol, 1 random follow point and 1 spawn point along with the NPC
/// and sets their parent and position same as the NPC.
/// Lastly, it assigns the instantiated patrol, random and spawn points to the spawned NPC.NPCStateMachine
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
