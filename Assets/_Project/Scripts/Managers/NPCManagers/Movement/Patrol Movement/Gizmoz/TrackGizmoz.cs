using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // this script willl now run both in Play Mode and in Edit Mode
public class TrackGizmos : MonoBehaviour
{
    public List<GameObject> TrackPoints;
    public GameObject TrackPointsFolder;
    public Color GizmosColor;
    public bool ShowGizmoz;

    public void CreateAreaPoint() // will run on pressing the button
    {
        GameObject pointInstance = new GameObject("Point");
        pointInstance.transform.SetParent(TrackPointsFolder.transform);
        pointInstance.transform.localPosition = Vector3.zero;

        TrackPoints.Add(pointInstance);
    }

    private void OnDrawGizmos()
    {
        if (!ShowGizmoz) return;

        Gizmos.color = GizmosColor;

        for (int i = 0; i < TrackPoints.Count; i++) // loop through every point in Area Points
        {
            Gizmos.DrawSphere(TrackPoints[i].transform.position, 1f); // Draw a Sphere on every point (GameObject) in the List<AreaPoints>

            // join every point (gameObject) in the List<AreaPoints> with a line

            if (i == TrackPoints.Count - 1) // if its the last point, then join it with the first point (AreaPoints[0]) to complete the loop 
            {
                Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[0].transform.position);
            }
            else // otherwise Join every point with the next point
            {
                Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[i + 1].transform.position);
            }
        }
    }
}