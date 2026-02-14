using UnityEngine;

public class AmIBeenWatched_E : MonoBehaviour
{
    public PlayerSight_E playerSight;

    public bool isBeingWatched = false;

    private void Update()
    {
        if (playerSight != null)
        {
            isBeingWatched = playerSight.IsImposterDetected(gameObject);
        }
    }
}
