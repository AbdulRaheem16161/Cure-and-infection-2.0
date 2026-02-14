using UnityEngine;

public class LadderTrigger : MonoBehaviour
{
    #region Ladder Detection
    public bool IsNearLadder = false;
    public Transform ClosestLadderCenter;
    #endregion

    #region Settings
    [SerializeField] private string ladderTag = "Ladder";
    [SerializeField] private string LadderCenterTag = "LadderCenter";
    #endregion

    #region Debug Gizmos
    [Header("Debug Settings")]
    [SerializeField] private bool showGizmos = true; // Toggle gizmos on/off
    #endregion

    private void Update()
    {
        UpdateClosestLadder();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ladderTag))
        {
            IsNearLadder = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(ladderTag))
        {
            IsNearLadder = false;
        }
    }

    private void UpdateClosestLadder()
    {
        GameObject[] ladders = GameObject.FindGameObjectsWithTag(LadderCenterTag);

        float closestDist = Mathf.Infinity;
        Transform closest = null;

        foreach (GameObject ladder in ladders)
        {
            float dist = Vector3.Distance(transform.position, ladder.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = ladder.transform;
            }
        }

        ClosestLadderCenter = closest;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return; // 🔥 Only draw when enabled

        // Draw a sphere at the closest ladder center
        if (ClosestLadderCenter != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ClosestLadderCenter.position, 0.3f);

            // Draw a line from the player to the ladder
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, ClosestLadderCenter.position);
        }
    }
}
