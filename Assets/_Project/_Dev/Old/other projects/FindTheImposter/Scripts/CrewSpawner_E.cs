using System.Linq;
using UnityEngine;

public class CrewSpawner_E : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject amungUsCrewPrefab;
    public GameObject pointToFollowPrefab;
    public GameObject player;

    [Header("Rect Spawn Settings")]
    public float spawnWidth = 10f;
    public float spawnLength = 10f;
    public Color gizmoColor = Color.green;
    public Vector3 spawnCenterOffset = Vector3.zero;

    public void SpawnACrewMate()
    {
        if (amungUsCrewPrefab == null || pointToFollowPrefab == null || player == null)
        {
            Debug.LogWarning("[CrewSpawner_E] One or more required references are missing!");
            return;
        }

        // Get next crew number based on existing ones
        int nextCrewNumber = GetNextAvailableNumber("Crew Mate");
        int nextPointNumber = GetNextAvailableNumber("Point To Follow");

        // Random spawn positions
        Vector3 crewSpawnPos = GetRandomPositionInRectangle();
        Vector3 pointSpawnPos = GetRandomPositionInRectangle();

        // Instantiate
        GameObject newCrew = Instantiate(amungUsCrewPrefab, crewSpawnPos, Quaternion.identity);
        GameObject newPoint = Instantiate(pointToFollowPrefab, pointSpawnPos, Quaternion.identity);

        // Name and tag crew
        newCrew.name = $"Crew Mate {nextCrewNumber}";
        newCrew.tag = "Crew Mate";

        // Name the follow point
        newPoint.name = $"Point To Follow {nextPointNumber}";

        // Assign follow target
        if (newCrew.TryGetComponent(out NavAgentController_E navAgent))
            navAgent.target = newPoint.transform;

        // Assign disguise settings
        if (newCrew.TryGetComponent(out ImposterDisguise_E disguise))
        {
            disguise.pointToFollowForDisguise = newPoint.transform;
        }


        // Assign AmIBeenWatched settings
        if (newCrew.TryGetComponent(out AmIBeenWatched_E amIBeenWatched))
        {
            amIBeenWatched.playerSight = player.GetComponent<PlayerSight_E>();
        }

        Debug.Log($"[CrewSpawner_E] Spawned Crew Mate {nextCrewNumber} and Point To Follow {nextPointNumber}");
    }

    private int GetNextAvailableNumber(string prefix)
    {
        var all = FindObjectsOfType<GameObject>()
            .Where(go => go.name.StartsWith(prefix))
            .Select(go => {
                string[] split = go.name.Split(' ');
                return int.TryParse(split.Last(), out int num) ? num : 0;
            });

        return (all.Any() ? all.Max() + 1 : 1);
    }

    private Vector3 GetRandomPositionInRectangle()
    {
        float x = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        float z = Random.Range(-spawnLength / 2f, spawnLength / 2f);
        return transform.position + spawnCenterOffset + new Vector3(x, 0f, z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Vector3 center = transform.position + spawnCenterOffset;
        Vector3 size = new Vector3(spawnWidth, 0.1f, spawnLength);
        Gizmos.DrawWireCube(center, size);
    }
}
