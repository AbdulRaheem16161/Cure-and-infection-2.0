//using System.Collections;
//using Game.MyNPC;
//using UnityEngine;

//public class NPCSpawner : MonoBehaviour
//{
//    public NPCRandomMoveManager RandomMoveManager;
//    public NPCPatrolManager PatrolMoveManager;

//    public GameObject Pedestrian;
//    public GameObject Guard;
//    public GameObject Zombie;

//    public GameObject PatrolFollowPoint;
//    public GameObject RandomFollowPoint;

//    public void Spawn(string NPCType)
//    {
//        #region Spawn NPC
//        GameObject _npcInstance = null;
//        GameObject _patrolPointInstance;
//        GameObject _randomPointInstance;

//        switch (NPCType)
//        {
//            case "Pedestrian":
//                _npcInstance = Instantiate(Pedestrian, transform);
//                break;

//            case "Guard":
//                _npcInstance = Instantiate(Guard, transform);
//                break;

//            case "Zombie":
//                _npcInstance = Instantiate(Zombie, transform);
//                break;
//        }

//        _npcInstance.transform.localPosition = Vector3.zero;

//        #endregion

//        #region Setting Up Patrol and Random Movement

//        _patrolPointInstance = Instantiate(PatrolFollowPoint, transform);
//        _randomPointInstance = Instantiate(RandomFollowPoint, transform);

//        _npcInstance.GetComponent<NPCStateMachine>().PatrolFollowPoint = _patrolPointInstance.transform;
//        _npcInstance.GetComponent<NPCStateMachine>().RandomFollowPoint = _randomPointInstance.transform;

//        RandomMoveManager.RandomFollowPoints.Add(_randomPointInstance);

//        _patrolPointInstance.GetComponent<PatrolFollowPoint>().Manager = PatrolMoveManager;
//        _patrolPointInstance.GetComponent<PatrolFollowPoint>().ItsFollower = _npcInstance;

//        _randomPointInstance.GetComponent<RandomFollowPoint>().Manager = RandomMoveManager;
//        _randomPointInstance.GetComponent<RandomFollowPoint>().ItsFollower = _npcInstance;

//        #endregion
//    }
//}