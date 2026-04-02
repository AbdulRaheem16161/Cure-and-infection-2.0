#if UNITY_EDITOR
using Game.Core;
using Game.MyNPC;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static NpcDefinition;

[RequireComponent(typeof(NpcBeliefs))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(StatsHandler))]
public class NpcPerception : MonoBehaviour
{
	public NpcDefinition NpcDefinition {  get; private set; }
	public NpcBeliefs Beliefs { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public StatsHandler StatsHandler { get; private set; }

	public GameObject rayViewPoint;

	#region Runtime Vision Values
	[Header("Runtime Vision Values")]
	public float viewAngle;
	public float viewDistance;
	private bool glancing;
	public bool showVision = false;
	#endregion

	#region Runtime Target Detection
	[Header("Runtime Target Detection")]
	[ReadOnly] public bool IsTargetDetected;
	[ReadOnly] public TargetData DetectedTarget;
	[ReadOnly] public bool IsEatableTargetDetected;
	[ReadOnly] public TargetData EatableTarget;
	#endregion

	#region layer Masks
	[Header("Layer Masks")]
	[SerializeField] private LayerMask targetMask;
	[SerializeField] private LayerMask lineOfSightMask;
	#endregion

	private Color normalColor = Color.green;
	private Color detectedColor = Color.red;
	private readonly float colorAlpha = 0.25f;

	private readonly Collider[] ColliderHits = new Collider[100];
	private readonly RaycastHit[] RaycastHits = new RaycastHit[100];

	#region alert mode timer;
	private readonly float alertModeCooldown = 5f;
	private float alertModeTimer;
	#endregion

	public Coroutine alertModeCoroutine;

	#region detect target timer
	private readonly float detectTargetCooldown = 0.1f;
	private float detectTargetTimer;
	#endregion

	#region detect eatable target timer
	private readonly float detectEatableTargetCooldown = 0.5f;
	private float detectEatableTargetTimer;
	#endregion

	#region awake + Initialize
	private void Awake()
	{
		Beliefs = GetComponent<NpcBeliefs>();
		StateMachine = GetComponent<NPCStateMachine>();
		StatsHandler = GetComponent<StatsHandler>();

		StatsHandler.OnHit += InvestigateWhereHitFrom;
	}
	public void Initialize(NpcDefinition npcDefinition)
	{
		if (rayViewPoint == null)
			Debug.LogError("rayViewPoint null, assign empty object where vision raycasts should start from");

		NpcDefinition = npcDefinition;
		viewAngle = NpcDefinition.ViewAngle;
		viewDistance = NpcDefinition.ViewDistance;
		showVision = false;
	}
	#endregion

	private void OnDisable()
	{
		showVision = false;
	}
	private void OnDestroy()
	{
		StatsHandler.OnHit -= InvestigateWhereHitFrom;
	}

	/// <summary>
	/// once target is dead reset it to stop npc from investigating the npc they just killed + to st
	/// </summary>

	private void Update()
	{
		if (StatsHandler.LifeState == LifeState.dead) return;

		AlertModeTimer();
		UpdateVisionBasedOnAlertState(Beliefs.Alert);
		SearchForLivingTarget();
		SearchForEatableTarget();
	}

	/// <summary>
	/// things that should trigger investigations
	/// getting attacked by something or moosing target to attack
	/// hearing a sound (could specify sounds, or filter sounds made by player (if not zombie) or npcs on same team)
	/// </summary>

	#region npc investigation triggers
	private void InvestigateWhereHitFrom(DamageContext damageContext)
	{
		if (!Beliefs.Alert)
			Beliefs.InvestigateLocation = damageContext.Attacker.transform.position;
	}
	private void InvestigateLastSeenEnemyPosition(Vector3 position)
	{
		if (!Beliefs.Alert)
			Beliefs.InvestigateLocation = position;
	}
	public void InvestigateSound(Vector3 position)
	{
		if (!Beliefs.Alert)
			Beliefs.InvestigateLocation = position;
	}
	#endregion

	#region npc alert mode timer + state handler
	private void AlertModeTimer()
	{
		if (Beliefs.InAlertState)
			alertModeTimer = alertModeCooldown;
		else if (alertModeTimer > 0)
			alertModeTimer -= Time.deltaTime;

		Beliefs.Alert = Beliefs.InAlertState || alertModeTimer > 0;
	}
	private void UpdateVisionBasedOnAlertState(bool alert)
	{
		float angleMultiplier = alert ? NpcDefinition.ViewAngleMultiplier : 1f;
		float distanceMultiplier = alert ? NpcDefinition.ViewDistanceMultiplier : 1f;

		if (glancing)
			angleMultiplier = NpcDefinition.ViewAngleMultiplier + 0.5f;

		viewAngle = NpcDefinition.ViewAngle * angleMultiplier;
		viewDistance = NpcDefinition.ViewDistance * distanceMultiplier;
	}
	#endregion

	#region npc glance simulation
	public void SimulateNpcGlancing(float glanceDuration)
	{
		StartCoroutine(SimulateNpcGlancingAround(glanceDuration));
	}
	private IEnumerator SimulateNpcGlancingAround(float glanceDuration)
	{
		glancing = true;
		yield return new WaitForSeconds(glanceDuration);
		glancing = false;
	}
	#endregion

	#region timed target search types
	private void SearchForLivingTarget()
	{
		detectTargetTimer -= Time.deltaTime;
		if (detectTargetTimer > 0) return;
		detectTargetTimer = detectTargetCooldown;

		//skip looking if target already found
		if (IsTargetDetected)
		{
			(DetectedTarget, IsTargetDetected) = TrackTarget(DetectedTarget, LifeState.alive);
		}
		else
		{
			DetectedTarget = SearchForClosestTarget(LifeState.alive);

			if (DetectedTarget != null && DetectedTarget.StatsHandler != null)
				IsTargetDetected = true;
			else
				IsTargetDetected = false;
		}
	}
	private void SearchForEatableTarget()
	{
		if (StatsHandler.LifeState != LifeState.zombified) return;

		detectEatableTargetTimer -= Time.deltaTime;
		if (detectEatableTargetTimer > 0) return;
		detectEatableTargetTimer = detectEatableTargetCooldown;

		if (IsEatableTargetDetected)
		{
			(EatableTarget, IsEatableTargetDetected) = TrackTarget(DetectedTarget, LifeState.dead);
		}
		else
		{
			EatableTarget = SearchForClosestTarget(LifeState.dead);

			if (EatableTarget != null && EatableTarget.StatsHandler != null)
				IsEatableTargetDetected = true;
			else
				IsEatableTargetDetected = false;
		}
	}
	#endregion

	#region search for closest target base method
	/// <summary>
	/// base search method, returns closest valid target after line of sight and filter checks
	/// </summary>
	private TargetData SearchForClosestTarget(LifeState lifeState)
	{
		float closestSqrDistance = viewDistance * viewDistance;
		TargetData closestTarget = null;

		for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits, targetMask); i++)
		{
			Collider collider = ColliderHits[i];
			GameObject go = collider.gameObject;

			if (gameObject == go) continue;

			if (!go.TryGetComponent(out StatsHandler stats))
			{
				Debug.LogError("target has no StatsHandler component, object may have wrong physics layer or lacking component");
				continue;
			}

			if (!FilterSearch(stats, lifeState)) continue;

			Vector3 dirToTarget = (collider.bounds.center - rayViewPoint.transform.position).normalized;
			if (!TargetInVisionConeAngle(dirToTarget)) continue;

			if (!TargetInLineOfSight(dirToTarget, lineOfSightMask, collider)) continue;

			float sqrDistance = (stats.transform.position - transform.position).sqrMagnitude;

			if (sqrDistance < closestSqrDistance)
			{
				closestSqrDistance = sqrDistance;
				closestTarget = new TargetData(stats, collider, stats.transform);
				closestTarget.UpdateTargetDistance(transform.position);
			}
		}

		return closestTarget;
	}
	#endregion

	#region search type filter and vision checks
	private bool FilterSearch(StatsHandler target, LifeState requiredLifeState)
	{
		//filter null and teams
		if (target == null) return false;
		if (target.Team != NPCSpawner.Teams.FreeFighter && target.Team == StateMachine.StatsHandler.Team) return false;

		//filter life state + special flags
		if (target.LifeState != requiredLifeState) return false;
		if (target.LifeState == LifeState.dead && !target.NpcDefinition.Flags.HasFlag(EntityFlags.canBecomeZombie))
			return false;

		return true;
	}
	private bool TargetInVisionConeAngle(Vector3 dirToTarget)
	{
		float dot = Vector3.Dot(transform.forward, dirToTarget);
		float cosHalfView = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);

		if (dot < cosHalfView)
			return false;
		else
			return true;
	}
	private bool TargetInLineOfSight(Vector3 dirToTarget, LayerMask mask, Collider collider)
	{
		int hitCount = Physics.RaycastNonAlloc(
			rayViewPoint.transform.position, dirToTarget, RaycastHits, viewDistance, mask, QueryTriggerInteraction.Ignore);

		float closestTargetDistance = viewDistance;
		float closestBlockingDistance = viewDistance;

		for (int i = 0; i < hitCount; i++)
		{
			RaycastHit hit = RaycastHits[i];

			if (hit.collider.gameObject == gameObject) continue; //ignore self
			if (hit.collider == collider)
			{
				if (hit.distance < closestTargetDistance)
					closestTargetDistance = hit.distance;
			}

			if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment"))
			{
				if (hit.distance < closestBlockingDistance)
					closestBlockingDistance = hit.distance;
			}
		}

		return closestTargetDistance < closestBlockingDistance;
	}
	#endregion

	#region handle tracking found targets and loosing them
	private (TargetData, bool) TrackTarget(TargetData trackedTarget, LifeState lifeState)
	{
		if (trackedTarget.StatsHandler.LifeState != lifeState) //life state changed (died or zombieifed now)
			return (null, false);

		trackedTarget.UpdateTargetDistance(transform.position);
		Vector3 dirToTarget = (trackedTarget.Collider.bounds.center - rayViewPoint.transform.position).normalized;
		if (TargetInVisionConeAngle(dirToTarget) && TargetInLineOfSight(dirToTarget, lineOfSightMask, trackedTarget.Collider)) 
			return (trackedTarget, true);

		InvestigateLastSeenEnemyPosition(trackedTarget.Transform.position);
		return (null, false); //no longer in vision cone or line of sight
	}
	#endregion

	private void OnDrawGizmos()
	{
		//draw vision cone for debugging
		if (!showVision) return;

		Color finalColor = IsTargetDetected ? detectedColor : normalColor;
		finalColor.a = colorAlpha;
		Handles.color = finalColor;

		Handles.DrawSolidArc(
			transform.position,
			Vector3.up,
			Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward,
			viewAngle,
			viewDistance
		);
	}
}
#endif

[Serializable]
public class TargetData
{
	public StatsHandler StatsHandler;
	public Collider Collider;
	public Transform Transform;
	public float Distance;

	public TargetData(StatsHandler statsHandler, Collider collider, Transform transform)
	{
		StatsHandler = statsHandler;
		Collider = collider;
		Transform = transform;
	}

	public void UpdateTargetDistance(Vector3 currentPosition)
	{
		Distance = (currentPosition - Transform.position).sqrMagnitude;
	}
}
