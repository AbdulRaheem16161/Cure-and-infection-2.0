using System;
using UnityEngine;

[Serializable]
public class TargetData
{
	public StatsHandler StatsHandler;
	public Collider Collider;
	public Vector3 Position => StatsHandler.transform.position;
	public Vector3 AimPoint => StatsHandler.AimPoint.position;
	public float SquaredDistance;

	public TargetData(StatsHandler statsHandler, Collider collider)
	{
		StatsHandler = statsHandler;
		Collider = collider;
	}

	public void UpdateTargetDistance(Vector3 currentPosition)
	{
		SquaredDistance = (currentPosition - Position).sqrMagnitude;
	}
}
