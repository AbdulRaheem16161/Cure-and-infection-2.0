using UnityEngine;
using System;
using System.Collections;

[ExecuteAlways]
public class RandomFollowPoint : MonoBehaviour
{
    public static event Action<GameObject> OnPointSpawned; // Delegate

    private void Awake()
    {
        StartCoroutine(ShoutOutAboutItsExistance());
    }

    private IEnumerator ShoutOutAboutItsExistance()
    {
        yield return null;

        // this adds its self to the RandomFollowPoints List of the RandomMovementManager
        OnPointSpawned?.Invoke(this.gameObject);  
    }
}
