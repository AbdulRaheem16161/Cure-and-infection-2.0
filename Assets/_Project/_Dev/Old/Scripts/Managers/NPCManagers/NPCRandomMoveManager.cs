//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class NPCRandomMoveManager : MonoBehaviour
//{
//    [Header("Attatchments")]
//    public GameObject RandomFollowPoint;
//    [SerializeField, ReadOnly] public GameObject Area;

//    public List<GameObject> RandomFollowPoints = new List<GameObject>();

//    [Header("Area Points (Polygon)")]
//    public List<GameObject> AreaPoints = new List<GameObject>();

//    [Header("Random Interval (seconds)")]
//    public float minDelay = 1f;
//    public float maxDelay = 3f;

//    [Header("Gizmo Settings")]
//    public Color gizmoColor = Color.green;
//    public float gizmoSphereSize = 0.2f;

//    public void Awake()
//    {
//        StartRandomRelocation();
//    }

//    public void StartRandomRelocation()
//    {
//        #region StartRandomRelocation
//        foreach (GameObject points in RandomFollowPoints)
//        {
//            if (points != null)
//                StartCoroutine(RelocateRoutine(points));
//        }
//        #endregion
//    }

//    public void AutoSetup()
//    {
//        #region AreaPoints Auto SetUp

//        // remove all null Spaces in the TrackPoints List
//        AreaPoints.RemoveAll(item => item == null);

//        if (AreaPoints.Count > 2)
//        {
//            return;
//        }
//        // if Track exists but Points dont exist then only create the points
//        else if (Area != null)
//        {
//            createAreaPoints();
//        }
//        // if neither Track nor its points exists, then create both
//        else
//        {
//            createArea();
//            createAreaPoints();
//        }
//        #endregion
//    }


//    private void createArea()
//    {
//        #region Create Track
//        Area = new GameObject("Area");
//        Area.transform.SetParent(transform);
//        Area.transform.localPosition = Vector3.zero;
//        #endregion
//    }

//    private void createAreaPoints()
//    {
//        #region Create Track Points
//        for (int i = 0; i < 4; i++)
//        {
//            GameObject AreaPoint = new GameObject("Area Point (" + (i + 1) + ")");
//            AreaPoint.transform.SetParent(Area.transform);
//            AreaPoints.Add(AreaPoint);
//            AreaPoint.transform.localPosition = GetSquareCornerPosition(i);
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

//    private IEnumerator RelocateRoutine(GameObject point)
//    {
//        #region Delay between Random relocation
//        while (true)
//        {
//            float delay = Random.Range(minDelay, maxDelay);
//            yield return new WaitForSeconds(delay);

//            point.transform.position = GetRandomPointInPolygon(point.transform.position.y);
//        }
//        #endregion
//    }


  
//    private Vector3 GetRandomPointInPolygon(float yHeight)
//    {
//        #region GetRandomPointInPolygon
//        /// <summary>
//        /// Returns a random walkable point inside the polygon formed by AreaPoints.
//        /// Uses NavMesh.SamplePosition to guarantee the point is valid.
//        /// </summary>

//        const float maxSampleDistance = 2f; // how far NavMesh can "adjust" the point
//        const int maxAttempts = 10;        // safety cap

//        for (int attempt = 0; attempt < maxAttempts; attempt++)
//        {
//            // Pick a random triangle inside the polygon
//            int i0 = 0; // fix first vertex
//            int i1 = Random.Range(1, AreaPoints.Count - 1);
//            int i2 = i1 + 1;

//            Vector3 a = AreaPoints[i0].transform.position;
//            Vector3 b = AreaPoints[i1].transform.position;
//            Vector3 c = AreaPoints[i2].transform.position;

//            // Random barycentric coordinates
//            float r1 = Random.value;
//            float r2 = Random.value;

//            if (r1 + r2 > 1f)
//            {
//                r1 = 1f - r1;
//                r2 = 1f - r2;
//            }

//            Vector3 randomPos = a + r1 * (b - a) + r2 * (c - a);
//            randomPos.y = yHeight;

//            // Check against NavMesh
//            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, maxSampleDistance, NavMesh.AllAreas))
//            {
//                return hit.position; // valid walkable point
//            }
//        }

//        Debug.LogWarning("Failed to find valid NavMesh point in polygon, returning fallback.");
//        return AreaPoints[0].transform.position; // fallback
//        #endregion
//    }

//    private void OnDrawGizmos()
//    {
//        #region Gizmoz
//        if (AreaPoints.Count < 2) return;

//        Gizmos.color = gizmoColor;

//        for (int i = 0; i < AreaPoints.Count; i++)
//        {
//            if (AreaPoints[i] != null)
//            {
//                Gizmos.DrawSphere(AreaPoints[i].transform.position, gizmoSphereSize);

//                if (i < AreaPoints.Count - 1 && AreaPoints[i + 1] != null)
//                    Gizmos.DrawLine(AreaPoints[i].transform.position, AreaPoints[i + 1].transform.position);

//                if (i == AreaPoints.Count - 1 && AreaPoints[0] != null)
//                    Gizmos.DrawLine(AreaPoints[i].transform.position, AreaPoints[0].transform.position);
//            }
//        }
//        #endregion
//    }
//}
