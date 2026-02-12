using System.Collections.Generic;
using UnityEngine;

public class PlayerSight_E : MonoBehaviour
{
    [Header("Vision Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)]
    public float viewAngle = 90f;

    [Header("Detection Settings")]
    [SerializeField] private LayerMask crewMateLayer;
    [SerializeField] private LayerMask obstacleMask;
    public string imposterTag = "Imposter";

    [SerializeField] public List<GameObject> imposters = new List<GameObject>();
    [SerializeField] public List<bool> isDetectedList = new List<bool>();

    private void Start()
    {
        InitializeImposters();
    }

    private void Update()
    {
        DetectionCone();
    }

    private void InitializeImposters()
    {
        imposters.Clear();
        isDetectedList.Clear();

        GameObject[] foundImposters = GameObject.FindGameObjectsWithTag(imposterTag);
        foreach (GameObject imposter in foundImposters)
        {
            imposters.Add(imposter);
            isDetectedList.Add(false);
        }
    }

    private void DetectionCone()
    {
        Vector3 eyeLevel = transform.position + Vector3.up * 0.8f; // simulate eyes or camera height

        for (int i = 0; i < imposters.Count; i++)
        {
            GameObject imposter = imposters[i];

            if (imposter == null)
            {
                isDetectedList[i] = false;
                continue;
            }

            Vector3 dirToTarget = (imposter.transform.position - eyeLevel).normalized;
            float distToTarget = Vector3.Distance(eyeLevel, imposter.transform.position);
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            bool inView = angle < viewAngle / 2f && distToTarget < viewRadius;

            bool isBlocked = Physics.Raycast(eyeLevel, dirToTarget, distToTarget, obstacleMask);

            // Set detection
            isDetectedList[i] = inView && !isBlocked;

            // Optional debug:
            Debug.DrawLine(eyeLevel, imposter.transform.position, isDetectedList[i] ? Color.green : Color.red);
        }
    }


    public bool IsImposterDetected(GameObject imposter)
    {
        int index = imposters.IndexOf(imposter);
        if (index >= 0 && index < isDetectedList.Count)
        {
            return isDetectedList[index];
        }

        Debug.LogWarning("Imposter not found: " + imposter.name);
        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, viewRadius);

        Vector3 origin = transform.position + Vector3.up * 0.2f;
        Vector3 leftBoundary = DirFromAngle(-viewAngle / 2);
        Vector3 rightBoundary = DirFromAngle(viewAngle / 2);

        Gizmos.DrawLine(origin, origin + leftBoundary * viewRadius);
        Gizmos.DrawLine(origin, origin + rightBoundary * viewRadius);
    }

    private Vector3 DirFromAngle(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
