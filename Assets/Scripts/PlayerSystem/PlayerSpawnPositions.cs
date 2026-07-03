using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPositions : MonoBehaviour
{
    public bool ShowSpawnPoints;
    public List<Transform> spawnPositions;

    private readonly System.Random systemRandom = new();

    public Vector3 GetRandomSpawnPosition()
    {
        float groundOffset = 1.5f;
        Vector3 position = spawnPositions[systemRandom.Next(0, spawnPositions.Count)].transform.position;
        return new Vector3(position.x, position.y + groundOffset, position.z);
    }

    private void OnDrawGizmos()
    {
        if (!ShowSpawnPoints) return;
        Gizmos.color = new(0.85f, 0.35f, 0.8f); ; // soft magenta

        for (int i = 0; i < spawnPositions.Count; i++)
            Gizmos.DrawSphere(spawnPositions[i].transform.position, 1f);
    }
}
