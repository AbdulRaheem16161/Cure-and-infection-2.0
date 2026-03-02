using UnityEngine;

public class PlayerAttack_E : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField, Range(0, 180)] private float attackAngle = 45f; // Half of full angle

    [Header("Gizmo Settings")]
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color gizmoSphereColor = new Color(1f, 0f, 0f, 0.2f); // Red transparent
    [SerializeField] private Color gizmoRayColor = Color.red;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Debug.Log("private void TryAttack");
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, LayerMask.GetMask("Crew Mate"));

        foreach (Collider hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget <= attackAngle)
            {
                DeathManager_E deathManager = hit.GetComponent<DeathManager_E>();
                if (deathManager != null)
                {
                    deathManager.Die();
                    Debug.Log($"[PlayerAttack_E] Attacked: {hit.name}");
                }
                else
                {
                    Debug.LogWarning($"[PlayerAttack_E] {hit.name} has no DeathManager_E component!");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;

        // Draw detection sphere
        Gizmos.color = gizmoSphereColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw forward attack cone limits
        Vector3 leftRay = Quaternion.Euler(0, -attackAngle, 0) * transform.forward;
        Vector3 rightRay = Quaternion.Euler(0, attackAngle, 0) * transform.forward;

        Gizmos.color = gizmoRayColor;
        Gizmos.DrawRay(transform.position, leftRay * attackRange);
        Gizmos.DrawRay(transform.position, rightRay * attackRange);
    }
}
