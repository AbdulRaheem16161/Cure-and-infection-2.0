using UnityEngine;
using System;
using System.Collections;

[ExecuteAlways]
public class RandomFollowPoint : MonoBehaviour
{
    public static event Action<GameObject> OnPointSpawned; // Delegate

    private void Awake()
    {
		OnPointSpawned?.Invoke(gameObject);
	}
}
