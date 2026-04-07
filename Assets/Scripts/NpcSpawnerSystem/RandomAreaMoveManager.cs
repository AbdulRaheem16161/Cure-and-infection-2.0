using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteAlways] // this script willl now run both in Play Mode and in Edit Mode
public class RandomAreaMoveManager : MonoBehaviour
{
    public List<GameObject> AreaPoints;
    public GameObject AreaPointsFolder; 
    public Color GizmosColor;
    public bool ShowGizmoz;

    public void CreateAreaPoint() // will run on pressing the button
    {
        GameObject pointInstance = new GameObject("Point");
        pointInstance.transform.SetParent(AreaPointsFolder.transform);
        pointInstance.transform.localPosition = Vector3.zero;

        AreaPoints.Add(pointInstance);  
    }

	public Vector3 GetRandomLocationInArea()
	{
		#region get a random point within the Area to teleport the follow point to
		List<float> zPositionsOfAreaPoints = new List<float>(); // make a list to store z component of positions of each area point
		List<float> xPositionsOfAreaPoints = new List<float>(); // make a list to store x component of positions of each area point

		for (int i = 0; i < AreaPoints.Count; i++)
		{
			// store z component of positions of each area point in the list
			zPositionsOfAreaPoints.Add(AreaPoints[i].transform.position.z);

			// store x component of positions of each area point in the list
			xPositionsOfAreaPoints.Add(AreaPoints[i].transform.position.x);
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

	private void OnDrawGizmos()
    {
        if (!ShowGizmoz) return;

        Gizmos.color = GizmosColor;

        for (int i = 0; i < AreaPoints.Count; i++) // loop through every point in Area Points
        {
            Gizmos.DrawSphere(AreaPoints[i].transform.position, 1f); // Draw a Sphere on every point (GameObject) in the List<AreaPoints>

            // join every point (gameObject) in the List<AreaPoints> with a line

            if (i == AreaPoints.Count - 1) // if its the last point, then join it with the first point (AreaPoints[0]) to complete the loop 
            {
                Gizmos.DrawLine(AreaPoints[i].transform.position, AreaPoints[0].transform.position);
            }
            else // otherwise Join every point with the next point
            {
                Gizmos.DrawLine(AreaPoints[i].transform.position, AreaPoints[i + 1].transform.position);
            }
        }
    }
}