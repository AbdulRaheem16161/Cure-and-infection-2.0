using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways] // this script willl now run both in Play Mode and in Edit Mode
public class PatrolPathManager : MonoBehaviour
{
    public List<GameObject> TrackPoints;
    public GameObject TrackPointsFolder;
    public bool ShowGizmoz;

    public void CreateNewPatrolPoint()
    {
		GameObject pointInstance = new($"Point{transform.childCount + 1}");
		pointInstance.transform.SetParent(transform);
		pointInstance.transform.localPosition = Vector3.zero;
		TrackPoints.Add(pointInstance);
	}

	public Vector3 GetNextPatrolPointLocation(int index)
	{
		return TrackPoints[index].transform.position;
	}

	private void OnDrawGizmos()
    {
        if (!ShowGizmoz) return;
		DrawPatrolPathPoints();
    }
    public void DrawPatrolPathPoints()
	{
		Gizmos.color = new(0.2f, 0.8f, 1f);

		for (int i = 0; i < TrackPoints.Count; i++)
		{
			Gizmos.DrawSphere(TrackPoints[i].transform.position, 1f); //draw a Sphere on every point

			if (i == TrackPoints.Count - 1) //join patrol points together
				Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[0].transform.position);
			else
				Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[i + 1].transform.position);
		}
	}
}