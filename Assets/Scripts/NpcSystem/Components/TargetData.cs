using System;
using UnityEngine;

[Serializable]
public class TargetData
{
	public StatsHandler StatsHandler;
	public Collider Collider;
	public Transform Transform;
	public float SquaredDistance;

	public TargetData(StatsHandler statsHandler, Collider collider, Transform transform)
	{
		StatsHandler = statsHandler;
		Collider = collider;
		Transform = transform;
	}

	public void UpdateTargetDistance(Vector3 currentPosition)
	{
		SquaredDistance = (currentPosition - Transform.position).sqrMagnitude;
	}
}
