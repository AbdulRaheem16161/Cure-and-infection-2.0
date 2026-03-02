/// <summary>
/// This script has buttons for each kind of NPC. Each button spawns the relevant NPC,
/// sets its parent to this (NPCSpawner) and its position to (0, 0, 0),
/// it also instantiates 1 patrol, 1 random follow point and 1 spawn point along with the NPC
/// and sets their parent and position same as the NPC.
/// Lastly, it assigns the instantiated patrol, random and spawn points to the spawned NPC.NPCStateMachine
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using Game.MyNPC;
using NUnit.Framework.Internal.Filters;

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

    #region Enums
    public enum Teams { Team1, Team2, Team3, Team4, Team5, Team6, Team7, Team8, FreeFighter }
    public Teams NPCsTeam;

    public WeaponsStruct.EnumWeaponType selectedWeapon;

    #endregion

    public void SpawnNPC(string NPCType)
    {
        #region Determine NPC to Spawn
        GameObject aiToSpawn = null;

        switch (NPCType)
        {
            case "TRex":
                aiToSpawn = TRex;
                break;
            case "Guard":
                aiToSpawn = Fighter;
                break;
            case "Zombie":
                aiToSpawn = Zombie;
                break;
            default:
                return;
        }
        #endregion

        #region Instantiate NPC and Points
        // Spawn NPC and set parent/position
        GameObject NPCInstance = generateGameObjectInstance(aiToSpawn);
        GameObject randomPointInstance = generateGameObjectInstance(RandomFollowPoint);
        GameObject patrolPointInstance = generateGameObjectInstance(PatrolFollowPoint);
        GameObject spawnPoint = generateGameObjectInstance(SpawnPoint);
        #endregion

        #region Assign NPCStateMachine References
        NPCStateMachine stateMachine = NPCInstance.GetComponent<NPCStateMachine>();

        // Assign follow points
        stateMachine.RandomFollowPoint = randomPointInstance.transform;
        stateMachine.PatrolFollowPoint = patrolPointInstance.transform;
        stateMachine.SpawnPoint = spawnPoint.transform;
        #endregion

        #region Set NPC's Team

        stateMachine.tag = NPCsTeam.ToString().Replace("Team", "Team "); // assign Team tag while changing Team(n) to Team (n)

        #region Assign Opponent Tags of the NPC
        List<string> NPCsTargetTags = new List<string>();

        foreach (Teams t in Enum.GetValues(typeof(Teams)))
        {
            // add all the teams of the Enum "Teams" in the TargetTags list except for its own team

            if (t == NPCsTeam && NPCsTeam != Teams.FreeFighter) // if its not a free fighter then skip its own tag
                continue;

            string teamName = t.ToString();  
            teamName = teamName.Replace("Team", "Team "); // change Team(n) to Team (n)

            NPCsTargetTags.Add(teamName);
        }

        stateMachine.TargetTags = NPCsTargetTags;

        #endregion

        #endregion

        #region Set NPC's Weapon
        if(aiToSpawn != Zombie)
        {
            stateMachine.WeaponHolder.GetComponent<NPCWeaponController>().selectedWeapon = selectedWeapon; //  zombie dont have a Weapon so
        }
     
        #endregion

        #region Assign PatrolFollowPoint References
        patrolPointInstance.GetComponent<PatrolFollowPoint>().ItsFollower = NPCInstance;
        patrolPointInstance.GetComponent<PatrolFollowPoint>().TrackGizmos = TrackGizmos;
        #endregion

        // Nothing to assign on RandomFollowPoint
    }

    private GameObject generateGameObjectInstance(GameObject GameObjectToSpawn)
    {
        #region Instantiate and Parent
        GameObject Instance = Instantiate(GameObjectToSpawn, transform.position, Quaternion.identity);
        Instance.transform.SetParent(transform);
        Instance.transform.localPosition = Vector3.zero;
        return Instance;
        #endregion
    }
}
