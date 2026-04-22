#if UNITY_EDITOR
using Game.MyNPC;
using System.Collections;
using UnityEditor;
using UnityEngine;
using static EntityDefinition;

[RequireComponent(typeof(NpcBeliefs))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(StatsHandler))]
public class NpcPerception : MonoBehaviour
{
	public EntityDefinition Definition {  get; private set; }
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

	public enum TargetTrackResult
	{
		valid, invalid, lost
	}

	#region layer Masks (internal config)
	[Header("Layer Masks")]
	private LayerMask targetMask;
	private LayerMask lineOfSightMask;
	#endregion

	#region Runtime Targets
	[Header("Runtime Targets")]
	public TargetData Target {get; private set;}
	public TargetData EatableTarget {get; private set;}
	public TargetData ClosestFleeTarget { get; private set;}
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

	#region flee target timer
	private readonly float fleeTargetCooldown = 0.25f;
	private float fleeTargetTimer;
	#endregion

	#region awake + Initialize
	private void Awake()
	{
		Beliefs = GetComponent<NpcBeliefs>();
		StateMachine = GetComponent<NPCStateMachine>();
		StatsHandler = GetComponent<StatsHandler>();

		targetMask = LayerMask.GetMask("CharacterDetection");
		lineOfSightMask = LayerMask.GetMask("Environment", "CharacterDetection");

		StatsHandler.OnHit += InvestigateWhereHitFrom;
	}
	public void Initialize(EntityDefinition definition)
	{
		if (rayViewPoint == null)
			Debug.LogError("rayViewPoint null, assign empty object where vision raycasts should start from");

		Definition = definition;
		viewAngle = Definition.ViewAngle;
		viewDistance = Definition.ViewDistance;
	}
	#endregion

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
		SearchForClosestFleeTarget();
	}

	/// <summary>
	/// things that should trigger investigations
	/// getting attacked by something or moosing target to attack
	/// hearing a sound (could specify sounds, or filter sounds made by player (if not zombie) or npcs on same team)
	/// </summary>

	#region npc investigation triggers
	private void InvestigateWhereHitFrom(DamageContext damageContext)
	{
		if (damageContext.Attacker == gameObject) return; //ignore damage from self
		if (!Beliefs.Alert)
			Beliefs.SetNewInvestigateLocation(damageContext.Attacker.transform.position);
	}
	private void InvestigateLastSeenEnemyPosition(Vector3 position)
	{
		if (Beliefs.Target == null)
			Beliefs.SetNewInvestigateLocation(position);
	}
	public void InvestigateSound(Vector3 position)
	{
		if (!Beliefs.Alert)
			Beliefs.SetNewInvestigateLocation(position);
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
		float angleMultiplier = alert ? Definition.ViewAngleMultiplier : 1f;
		float distanceMultiplier = alert ? Definition.ViewDistanceMultiplier : 1f;

		if (glancing)
			angleMultiplier = Definition.ViewAngleMultiplier + 0.5f;

		viewAngle = Definition.ViewAngle * angleMultiplier;
		viewDistance = Definition.ViewDistance * distanceMultiplier;
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
		if (Target != null)
		{
			Vector3 lastRecordedPosition = Beliefs.Target.Transform.position;
			TargetTrackResult trackResult;
			(Target, trackResult) = TrackTarget(Target, LifeState.alive);

			if (trackResult == TargetTrackResult.lost)
				InvestigateLastSeenEnemyPosition(lastRecordedPosition);
		}
		else
			Target = SearchForClosestTarget(LifeState.alive);
	}
	private void SearchForEatableTarget()
	{
		if (StatsHandler.LifeState != LifeState.zombified) return;

		detectEatableTargetTimer -= Time.deltaTime;
		if (detectEatableTargetTimer > 0) return;
		detectEatableTargetTimer = detectEatableTargetCooldown;

		if (EatableTarget != null)
		{
			TargetTrackResult trackResult;
			(EatableTarget, trackResult) = TrackTarget(EatableTarget, LifeState.dead);
		}
		else
			EatableTarget = SearchForClosestTarget(LifeState.dead);
	}
	private void SearchForClosestFleeTarget()
	{
		fleeTargetTimer -= Time.deltaTime;
		if (fleeTargetTimer > 0) return;
		fleeTargetTimer = fleeTargetCooldown;

		ClosestFleeTarget = SearchForClosestTarget(LifeState.alive);
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
		static bool LifeStateValid(StatsHandler target, LifeState requiredLifeState)
		{
			if (requiredLifeState == LifeState.alive && target.LifeState != LifeState.dead) return true;

			else if (requiredLifeState == LifeState.dead && target.LifeState == LifeState.dead && 
				target.Definition.Flags.HasFlag(EntityFlags.canBecomeZombie)) return true;

			else return false;
		}

		return LifeStateValid(target, requiredLifeState);
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
	private (TargetData, TargetTrackResult) TrackTarget(TargetData trackedTarget, LifeState lifeState)
	{
		if (trackedTarget.StatsHandler.LifeState != lifeState) //life state changed (died or zombiefied now)
			return (null, TargetTrackResult.invalid);

		trackedTarget.UpdateTargetDistance(transform.position);
		Vector3 dirToTarget = (trackedTarget.Collider.bounds.center - rayViewPoint.transform.position).normalized;
		if (TargetInVisionConeAngle(dirToTarget) && TargetInLineOfSight(dirToTarget, lineOfSightMask, trackedTarget.Collider)) 
			return (trackedTarget, TargetTrackResult.valid);

		return (null, TargetTrackResult.lost); //no longer in vision cone or line of sight
	}
	#endregion

	private void OnDrawGizmos()
	{
		//draw vision cone for debugging
		if (!showVision) return;

		Color finalColor = Beliefs.Target != null ? detectedColor : normalColor;
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
