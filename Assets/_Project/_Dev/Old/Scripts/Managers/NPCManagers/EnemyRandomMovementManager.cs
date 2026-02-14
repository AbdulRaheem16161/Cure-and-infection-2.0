using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AiRandomMovementManager : MonoBehaviour
{
    [Header("Movement Area")]
    [SerializeField] private float width = 10f;
    [SerializeField] private float length = 10f;
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Random Interval")]
    [SerializeField] private float minDelay = 1f;
    [SerializeField] private float maxDelay = 3f;

    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    private Vector3 AreaCenter => transform.position;

    private void Start()
    {
        AddAllTaggedPointsToList();
    }

    private void AddAllTaggedPointsToList()
    {
        GameObject[] taggedPoints = GameObject.FindGameObjectsWithTag("Point To Follow");

        if (taggedPoints.Length == 0)
        {
            Debug.LogWarning("PointToFollowManager_E: No GameObjects with tag 'Point To Follow' found in the scene.");
            return;
        }

        foreach (GameObject obj in taggedPoints)
        {
            if (obj.activeInHierarchy)
            {
                targets.Add(obj);
                StartCoroutine(RelocateRoutine(obj));
            }
        }
    }

    private IEnumerator RelocateRoutine(GameObject target)
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            Vector3 newPosition = GetRandomPointInArea(target.transform.position.y);
            target.transform.position = newPosition;
        }
    }

    private Vector3 GetRandomPointInArea(float yHeight)
    {
        float halfWidth = width / 2f;
        float halfLength = length / 2f;

        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomZ = Random.Range(-halfLength, halfLength);

        return new Vector3(AreaCenter.x + randomX, yHeight, AreaCenter.z + randomZ);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireCube(AreaCenter + Vector3.up * 0.1f, new Vector3(width, 0.1f, length));
    }
}
