using UnityEngine;

[ExecuteAlways]
public class EnemyPatrolManager : MonoBehaviour
{
    #region Variables
    [Header("Target Object")]
    public GameObject Enemy;

    [Header("Track Settings")]
    public Transform[] TrackPoints;

    [Header("Target Object")]
    public Transform PointToFollow;

    [Header("Trigger Settings")]
    public string requiredEnemyName;

    [Header("Looping")]
    public bool Loop = true;

    [Header("Gizmo Settings")]
    public bool ShowTrack = true;
    public Color TrackColor = Color.blue;

    private int currentPointIndex = 0;

    #endregion

    #region Auto Setup
    public void AutoSetup()
    {
        SetupPatrolTrack();
        SetupPointToFollow();
        SetupRigidbodyAndForwarder();
        requiredEnemyName = Enemy.gameObject.name;

        Debug.Log("<color=orange>Patrol setup completed! ✅</color>");
    }

    #region Patrol Track Setup
    private void SetupPatrolTrack()
    {
        Transform trackParent = transform.Find("PatrolTrack");
        if (!trackParent)
        {
            trackParent = new GameObject("PatrolTrack").transform;
            trackParent.SetParent(transform);
            trackParent.localPosition = Vector3.zero;
        }

        int existingPoints = trackParent.childCount;

        if (existingPoints == 0)
        {
            TrackPoints = new Transform[4];
            float spacing = 2f;

            Vector3[] squarePositions = new Vector3[]
            {
                new Vector3(0f, 0f, 0f),
                new Vector3(spacing, 0f, 0f),
                new Vector3(spacing, 0f, spacing),
                new Vector3(0f, 0f, spacing)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject point = new GameObject($"Point {i + 1}");
                point.transform.SetParent(trackParent);
                point.transform.localPosition = squarePositions[i];
                TrackPoints[i] = point.transform;
            }
        }
        else
        {
            TrackPoints = new Transform[existingPoints];
            for (int i = 0; i < existingPoints; i++)
            {
                TrackPoints[i] = trackParent.GetChild(i);
            }
        }
    }
    #endregion

    #region PointToFollow Setup
    private void SetupPointToFollow()
    {
        if (!PointToFollow)
        {
            Transform existingFollow = transform.Find("PointToFollow");
            if (!existingFollow)
            {
                GameObject followObj = new GameObject("PointToFollow");
                followObj.transform.SetParent(transform);
                followObj.transform.localPosition = Vector3.zero;

                BoxCollider col = followObj.AddComponent<BoxCollider>();
                col.isTrigger = true;

                PointToFollow = followObj.transform;
            }
            else
            {
                PointToFollow = existingFollow;
                BoxCollider col = PointToFollow.GetComponent<BoxCollider>();
                if (!col) col = PointToFollow.gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }
        }
    }
    #endregion

    #region Rigidbody & Forwarder Setup
    private void SetupRigidbodyAndForwarder()
    {
        Rigidbody rb = PointToFollow.GetComponent<Rigidbody>();
        if (!rb) rb = PointToFollow.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        if (!PointToFollow.GetComponent<PointTriggerForwarder>())
        {
            PointTriggerForwarder forwarder = PointToFollow.gameObject.AddComponent<PointTriggerForwarder>();
            forwarder.manager = this;
        }
    }
    #endregion

    #endregion

    #region Runtime
    private void Start()
    {
        if (TrackPoints != null && TrackPoints.Length > 0 && PointToFollow != null)
        {
            PointToFollow.position = TrackPoints[0].position;
        }
    }

    public void TeleportToNextPoint()
    {
        if (TrackPoints == null || TrackPoints.Length == 0 || PointToFollow == null) return;

        currentPointIndex++;

        if (currentPointIndex >= TrackPoints.Length)
        {
            if (Loop)
                currentPointIndex = 0;
            else
                return;
        }

        PointToFollow.position = TrackPoints[currentPointIndex].position;
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!ShowTrack || TrackPoints == null || TrackPoints.Length == 0) return;

        Gizmos.color = TrackColor;

        for (int i = 0; i < TrackPoints.Length; i++)
        {
            if (TrackPoints[i] != null)
            {
                Gizmos.DrawSphere(TrackPoints[i].position, 0.2f);

                if (i < TrackPoints.Length - 1 && TrackPoints[i + 1] != null)
                    Gizmos.DrawLine(TrackPoints[i].position, TrackPoints[i + 1].position);

                if (Loop && i == TrackPoints.Length - 1 && TrackPoints[0] != null)
                    Gizmos.DrawLine(TrackPoints[i].position, TrackPoints[0].position);
            }
        }
    }
    #endregion

    #region Helper Classes
    private class PointTriggerForwarder : MonoBehaviour
    {
        public EnemyPatrolManager manager;

        private void OnTriggerEnter(Collider other)
        {
            if (other.name == manager.requiredEnemyName)
            {
                manager.TeleportToNextPoint();
            }
        }
    }
    #endregion
}
