#if UNITY_EDITOR
using System.Collections;
using Game.MyNPC;
using UnityEditor;
using UnityEngine;

public class DetectionCone : MonoBehaviour
{
    #region Settings
    [Header("General Settings")]
    public float viewAngle = 45f;
    public float viewDistance = 5f;
    public bool showGizmos = false;
    public NPCStateMachine stateMachine;
    [Space(10)]
    #endregion

    #region AlertMode Settings
    [Header("Alert Mode Settings")]
    public float HighAlertDuration = 3f;
    public float ViewDistanceMultiplier = 2f;
    public bool isInAlertMode;
    public Coroutine alertModeCoroutine;
    [Space(10)]
    #endregion

    #region Colors
    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private float colorAlpha = 0.25f;
    [Space(10)]
    #endregion

    #region Runtime Values
    [Header("Runtime Values")]
    [SerializeField, ReadOnly] public bool isTargetDetected;
    [SerializeField, ReadOnly] public GameObject DetectedTarget;
    #endregion

    private void Update()
    {
        #region summary
        /// <summary>
        /// Runs every frame to find the closest valid target
        /// inside the vision cone and update detection state.
        /// Clears detection when nothing valid is found.
        /// </summary>
        #endregion

        if (isInAlertMode) return;

        #region Update Detected Target

        GameObject closestTarget = GetClosestValidTarget();

        if (closestTarget != null)
        {
            DetectedTarget = closestTarget;
            isTargetDetected = true;
        }
        else
        {
            DetectedTarget = null;
            isTargetDetected = false;
        }

        #endregion
    }

    private GameObject GetClosestValidTarget()
    {
        #region summary
        /// <summary>
        /// Checks all colliders within view distance and filters
        /// only those inside the vision cone.
        /// Returns the nearest valid target found.
        /// </summary>
        #endregion

        #region GetClosestValidTarget

        Collider[] hits = Physics.OverlapSphere(transform.position, viewDistance);

        float closestDistance = float.MaxValue;
        GameObject closestTarget = null;

        foreach (var hit in hits)
        {
            // Tag filter
            if (!stateMachine.TargetTags.Contains(hit.tag) || hit.gameObject == this.gameObject)
                continue;

            Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            // Cone check
            if (angle > viewAngle * 0.5f) // Half-angle used because Vector3.Angle measures from center, not edges
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            // Closest selection
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = hit.gameObject;
            }
        }

        return closestTarget;
        #endregion
    }

    private void Awake()
    {
        #region Awake

        // if chase is disabled, disable gizmos
        if (!stateMachine.EnableChase)
            showGizmos = false;

        #endregion
    }

    private void OnDisable()
    {
        #region OnDisable

        // Disable gizmos
        showGizmos = false;

        #endregion
    }

    public void EnableAlertMode(GameObject Attacker)
    {
        #region Trigger Alert Mode Coroutine

        //if (alertModeCoroutine != null) return; // prevents multiple coroutines

        alertModeCoroutine = StartCoroutine(AlertModeCoroutine(Attacker));

        #endregion
    }

    public IEnumerator AlertModeCoroutine(GameObject Attacker)
    {
        #region Start Alert mode

        isInAlertMode = true;
        float defaultViewAngle = viewAngle;
        float defaultViewDistance = viewDistance;

        DetectedTarget = Attacker;
        isTargetDetected = true;

        viewAngle = 360;
        viewDistance = viewDistance * ViewDistanceMultiplier;
        yield return new WaitForSeconds(HighAlertDuration);

        viewAngle = defaultViewAngle;
        viewDistance = defaultViewDistance;
        isInAlertMode = false;

        #endregion
    }

    private void OnDrawGizmos()
    {
        #region OnDrawGizmos
        #region summary
        /// <summary>
        /// Draws the vision cone in the editor to visualize
        /// NPC awareness and detection state.
        /// </summary>
        #endregion

        if (!showGizmos) return;

        Color finalColor = isTargetDetected ? detectedColor : normalColor;
        finalColor.a = colorAlpha;
        Handles.color = finalColor;

        Handles.DrawSolidArc(
            transform.position,
            Vector3.up,
            Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward,
            viewAngle,
            viewDistance
        );

        #endregion
    }
}
#endif
