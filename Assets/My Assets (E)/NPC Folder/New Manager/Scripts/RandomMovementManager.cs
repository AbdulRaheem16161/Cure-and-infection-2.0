using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Linq;
using System.Drawing;

[ExecuteAlways]
public class RandomMovementManager : MonoBehaviour
{
   public AreaGizmos areaGizmos;
   public List<GameObject> RandomFollowPoints;

    public int MinDuration;
    public int MaxDuration;

    void Awake()
    {
        RandomFollowPoint.OnPointSpawned += AddPointInList;
    }

    public void Start()
    {
        for (int i = 0; i < RandomFollowPoints.Count; i++)
        {
            StartCoroutine(TeleportRandomFollowPoint(RandomFollowPoints[i]));
        }
    }

    private void RestartTeleportationCoroutines()
    {
        if (Application.isPlaying)
        { 
            StopAllCoroutines();

            for (int i = 0; i < RandomFollowPoints.Count; i++)
            {
                StartCoroutine(TeleportRandomFollowPoint(RandomFollowPoints[i]));
            }
        }
    }

    private IEnumerator TeleportRandomFollowPoint(GameObject point)
    {
       while(true)
       {
            float randomDuration = Random.Range(MinDuration, MaxDuration);
            point.transform.position = TeleportPosition();
            yield return new WaitForSeconds(randomDuration);
       }
    }

    private Vector3 TeleportPosition()
    {
        #region get a random point within the Area to teleport the follow point to
        List<float> zPositionsOfAreaPoints = new List<float>(); // make a list to store z component of positions of each area point
        List<float> xPositionsOfAreaPoints = new List<float>(); // make a list to store x component of positions of each area point

        for (int i = 0; i < areaGizmos.AreaPoints.Count; i++)
        {
            // store z component of positions of each area point in the list
            zPositionsOfAreaPoints.Add(areaGizmos.AreaPoints[i].transform.position.z);
      
            // store x component of positions of each area point in the list
            xPositionsOfAreaPoints.Add(areaGizmos.AreaPoints[i].transform.position.x);
        }

        // store the largest and smallest values of x and z components of all the area points stored in the list.
        // the largest and smallest values will serve as boundries of the Area within which the random movement will be done.
        float largestValueX = xPositionsOfAreaPoints.Max();
        float largestValueZ = zPositionsOfAreaPoints.Max();
        float SmallesValueX = xPositionsOfAreaPoints.Min();
        float SmallesValueZ = zPositionsOfAreaPoints.Min();

        // get a random x and z value between the largest and smallest values (bounries).
        // this will help to get a random point within the area to teleport to
        float RandomValueX = Random.Range(SmallesValueX, largestValueX);
        float RandomValueZ = Random.Range(SmallesValueZ, largestValueZ);

        // store that random point
        Vector3 TeleportPosition = new Vector3(RandomValueX, 0f, RandomValueZ);
        #endregion

        return TeleportPosition;
    }

    private void AddPointInList(GameObject point)
    {
        RandomFollowPoints.Add(point);

        // everytime a new point is added in the list (that ofcourse happens when a new NPC is spawned) restart the teleportation coroutines
        // so that the newly added point dont get ignored
        RestartTeleportationCoroutines();
    }
}
