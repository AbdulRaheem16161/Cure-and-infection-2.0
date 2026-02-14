using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // this script willl now run both in Play Mode and in Edit Mode
public class AreaGizmos : MonoBehaviour
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