//#region Helper Classes
//using UnityEngine;

//public class PatrolFollowPoint : MonoBehaviour
//{
//    public NPCPatrolManager Manager;
//    public GameObject ItsFollower;
//    private int index = 0;


//    private void Awake()
//    {
//        if (Manager != null && Manager.TrackPoints.Count > 0)
//        {
//            // teleport to the position of the first track point
//            transform.position = Manager.TrackPoints[index].transform.position;
//        }
//        else
//        {
//            Debug.LogWarning("⚠ No Manager or TrackPoints found!");
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        //  only react if the correct NPC touches
//        if (other.gameObject == ItsFollower)
//        {
//            index++;

//            //  make sure we don’t go out of bounds
//            if (index < Manager.TrackPoints.Count)
//            {
//                transform.position = Manager.TrackPoints[index].transform.position;
//            }
//            else
//            {
//                // loop back to start
//                index = 0;
//                transform.position = Manager.TrackPoints[index].transform.position;
//            }
//        }
//    }
//}
//#endregion
