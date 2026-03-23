using System.Collections.Generic;
using UnityEngine;

public class MarchingMovementManager : MonoBehaviour
{
    #region Settings
    [SerializeField] private float moveSpeed = 3f;
    #endregion

    #region Track & Follow Points
    public List<GameObject> trackPoints = new List<GameObject>();
    public List<GameObject> followPoints = new List<GameObject>();
    public int trackPointCount = 0;
    #endregion

    #region Prefabs
    public GameObject trackPointPrefab;
    public GameObject followPointPrefab;
    #endregion

    #region Runtime Values
    private List<int> followPointTargetIndices = new List<int>();
    #endregion

    private void Awake()
    {
        #region Awake
        InitializeFollowPointTargets();
        #endregion
    }

    private void Update()
    {
        #region Update
        MoveFollowPoints();
        #endregion
    }

    #region Initialization
    void InitializeFollowPointTargets()
    {
        #region summary
        /// <summary>
        /// Assigns each followPoint its next trackPoint index
        /// so movement starts correctly at game start
        /// </summary>
        #endregion

        followPointTargetIndices.Clear();

        for (int i = 0; i < followPoints.Count; i++)
        {
            followPointTargetIndices.Add(i + 1);
        }
    }
    #endregion

    #region Movement
    void MoveFollowPoints()
    {
        #region summary
        /// <summary>
        /// Updates movement for all followPoints every frame
        /// </summary>
        #endregion

        for (int i = 0; i < followPoints.Count; i++)
        {
            MoveSingleFollowPoint(i);
        }
    }

    void MoveSingleFollowPoint(int index)
    {
        #region summary
        /// <summary>
        /// Moves one followPoint smoothly toward its next trackPoint
        /// Loops infinitely from start when reaching the last trackPoint
        /// </summary>
        #endregion

        int targetIndex = followPointTargetIndices[index];

        // Looping logic
        if (targetIndex >= trackPoints.Count)
        {
            // Reset position to start
            followPoints[index].transform.position = trackPoints[0].transform.position;
            followPointTargetIndices[index] = 1; // next target
            return;
        }

        Transform followPoint = followPoints[index].transform;
        Transform targetTrackPoint = trackPoints[targetIndex].transform;

        followPoint.position = Vector3.MoveTowards(
            followPoint.position,
            targetTrackPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(followPoint.position, targetTrackPoint.position) < 0.05f)
        {
            followPointTargetIndices[index]++;
        }
    }
    #endregion

    #region Track Point Generation
    [ContextMenu("Generate Track Point")]
    public void GenerateTrackPoint()
    {
        #region summary
        /// <summary>
        /// Creates a new trackPoint and a matching followPoint
        /// positioned correctly in sequence
        /// </summary>
        #endregion

        if (trackPointCount == 0)
        {
            GameObject firstTP = Instantiate(trackPointPrefab, transform.position, Quaternion.identity);
            firstTP.transform.parent = transform;
            trackPoints.Add(firstTP);
            trackPointCount++;

            GameObject firstFP = Instantiate(followPointPrefab);
            firstFP.transform.position = firstTP.transform.position;
            firstFP.transform.parent = transform;
            followPoints.Add(firstFP);

            return;
        }

        GameObject newTP = Instantiate(trackPointPrefab);
        newTP.transform.parent = transform;

        Vector3 pos = trackPoints[trackPointCount - 1].transform.position;
        pos.z += 5f;
        newTP.transform.position = pos;

        trackPoints.Add(newTP);
        trackPointCount++;

        GameObject newFP = Instantiate(followPointPrefab);
        newFP.transform.position = newTP.transform.position;
        newFP.transform.parent = transform;
        followPoints.Add(newFP);
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        #region summary
        /// <summary>
        /// Draws yellow lines between trackPoints for visualization
        /// </summary>
        #endregion

        if (trackPointCount <= 1) return;

        for (int i = 0; i < trackPointCount - 1; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                trackPoints[i].transform.position,
                trackPoints[i + 1].transform.position
            );
        }
    }
    #endregion
}
