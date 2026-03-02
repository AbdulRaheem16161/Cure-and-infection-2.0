//using System.Collections.Generic;
//using UnityEngine;

//public class NPCPatrolManager : MonoBehaviour
//{
//    #region Attatchments
//    [Header("Attatchments")]
//    [SerializeField, ReadOnly] public GameObject Track;
//    public GameObject PatrolFollowPoint;
//    #endregion

//    [Space(10)]
//    public List<GameObject> TrackPoints = new List<GameObject>();

//    #region Patrol Gizmoz Settings
//    public Color gizmoColor = Color.green;
//    public float sphereRadius = 0.5f;
//    #endregion

//    public void AutoSetup()
//    {
//        #region PatrolTrack AutoSetUp
//        // remove all null Spaces in the TrackPoints List
//        TrackPoints.RemoveAll(item => item == null);

//        // if Track and its points already exists then return
//        if (TrackPoints.Count > 1)
//        {
//            return;
//        }
//        // if Track exists but Points dont exist then only create the points
//        else if (Track != null)
//        {
//            createTrackPoints();
//        }
//        // if neither Track nor its points exists, then create both
//        else
//        {
//            createTrack();
//            createTrackPoints();
//        }
//        #endregion
//    }

//    private void createTrack()
//    {
//        #region Create Track
//        Track = new GameObject("Track");
//        Track.transform.SetParent(transform);
//        Track.transform.localPosition = Vector3.zero;
//        #endregion
//    }

//    private void createTrackPoints()
//    {
//        #region Create Track Points
//        for (int i = 0; i < 4; i++)
//        {
//            GameObject trackPoint = new GameObject("Track Point (" + (i + 1) + ")");
//            trackPoint.transform.SetParent(Track.transform);
//            TrackPoints.Add(trackPoint);
//            trackPoint.transform.localPosition = GetSquareCornerPosition(i);
//        }
//        #endregion
//    }

//    private Vector3 GetSquareCornerPosition(int i)
//    {
//        #region Get Square Corner Position 
//        float Distance = 5f;
//        Vector3[] squarePoints = new Vector3[]
//        {
//        new Vector3(-Distance, 0, -Distance), // bottom-left
//        new Vector3(-Distance, 0,  Distance), // top-left
//        new Vector3( Distance, 0,  Distance), // top-right
//        new Vector3( Distance, 0, -Distance)  // bottom-right
//        };

//        return (squarePoints[i]);
//        #endregion
//    }

//    #region Gizmos Drawing
//    private void OnDrawGizmos()
//    {
//        if (TrackPoints == null || TrackPoints.Count == 0) return;

//        Gizmos.color = gizmoColor;

//        for (int i = 0; i < TrackPoints.Count; i++)
//        {
//            if (TrackPoints[i] != null)
//            {
//                // draw spheres at each point
//                Gizmos.DrawSphere(TrackPoints[i].transform.position, sphereRadius);

//                // draw lines between consecutive points
//                if (i < TrackPoints.Count - 1 && TrackPoints[i + 1] != null)
//                {
//                    Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[i + 1].transform.position);
//                }

//                // close the loop (last point connects to first)
//                if (i == TrackPoints.Count - 1 && TrackPoints[0] != null)
//                {
//                    Gizmos.DrawLine(TrackPoints[i].transform.position, TrackPoints[0].transform.position);
//                }
//            }
//        }
//    }
//    #endregion

//}