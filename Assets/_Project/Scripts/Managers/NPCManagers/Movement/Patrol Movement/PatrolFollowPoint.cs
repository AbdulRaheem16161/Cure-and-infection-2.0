using System;
using UnityEngine;

public class PatrolFollowPoint : MonoBehaviour
{
    int TrackPointNum = 0;

    // both ItsFollower and TrackGizmos are assigned by the NPCSpawner on the time of spawning an NPC 
    public GameObject ItsFollower;
    public TrackGizmos TrackGizmos; 

    private void OnTriggerEnter(Collider other)
    {
        // whenever ItsFollower collides with it, it teleports to the next patrol point on the PatrolTrack
        if (other.gameObject == ItsFollower)
        {
            TrackPointNum++;
            if (TrackPointNum == TrackGizmos.TrackPoints.Count)
            {
                TrackPointNum = 0;
            }

            this.transform.position = TrackGizmos.TrackPoints[TrackPointNum].transform.position;
        }
    }
}
