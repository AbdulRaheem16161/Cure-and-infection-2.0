#if UNITY_EDITOR
using Game.Core;
using Game.MyNPC;
using System.Collections;
using System.Net;
using UnityEditor;
using UnityEngine;

public class NpcPerception : MonoBehaviour
{
	public NPCStateMachine StateMachine { get; private set; }

	#region Settings
	[Header("General Settings")]
	public GameObject rayViewPoint;
	private float baseViewAngle;
	private float baseviewDistance;
	public float viewAngle = 45f;
	public float viewDistance = 5f;
	public bool showGizmos = false;
	private bool isZombie;
	[Space(10)]
	#endregion

	#region AlertMode Settings
	[Header("Alert Mode Settings")]
	public float HighAlertDuration = 3f;
	public float ViewAngleMultiplier = 1.5f;
	public float ViewDistanceMultiplier = 2f;
	public bool isInAlertMode;
	public Coroutine alertModeCoroutine;
	[Space(10)]
	#endregion

	#region layerMasks
	[Header("Layer Masks")]
	[SerializeField] private LayerMask targetMask;
	[SerializeField] private LayerMask lineOfSightMask;
	#endregion

	#region Colors
	[Header("Colors")]
	[SerializeField] private Color normalColor = Color.green;
	[SerializeField] private Color detectedColor = Color.red;
	[SerializeField] private float colorAlpha = 0.25f;
	[Space(10)]
	#endregion

	#region Runtime Values
	[Header("Runtime Values")]
	private readonly Collider[] ColliderHits = new Collider[100];
	private readonly RaycastHit[] RaycastHits = new RaycastHit[100];
	public bool IsTargetDetected { get; private set; }
	public TargetData DetectedTarget { get; private set; }

	public TargetData LastKilledTarget { get; private set; }

	public bool IsEatableTargetDetected { get; private set; }
	public TargetData EatableTarget { get; private set; }
	#endregion

	#region search types
	public enum SearchType
	{
		alive, eatable
	}
	#endregion

	#region detect target timer
	private readonly float detectTargetCooldown = 0.1f;
	private float detectTargetTimer;
	#endregion

	#region detect eatable target timer
	private readonly float detectEatableTargetCooldown = 0.5f;
	private float detectEatableTargetTimer;
	#endregion

	public void Initialize(NpcDefinition npcDefinition, NPCStateMachine stateMachine)
	{
		if (rayViewPoint == null)
			Debug.LogError("rayViewPoint null, assign empty object where vision raycasts should start from");

		StateMachine = stateMachine;
		isZombie = npcDefinition.IsZombie;
		baseViewAngle = npcDefinition.ViewAngle;
		baseviewDistance = npcDefinition.ViewDistance;

		viewAngle = npcDefinition.ViewAngle;
		viewDistance = npcDefinition.ViewDistance;
		HighAlertDuration = npcDefinition.HighAlertDuration;
		ViewAngleMultiplier = npcDefinition.ViewAngleMultiplier;
		ViewDistanceMultiplier = npcDefinition.ViewDistanceMultiplier;

		if (!StateMachine.EnableChase)
			showGizmos = false;
	}

	private void OnDisable()
	{
		showGizmos = false;
	}

	/// <summary>
	/// once target is dead reset it to stop npc from investigating the npc they just killed + to st
	/// </summary>

	private void Update()
	{
		if (StateMachine.StatsHandler.IsDead) return;

		SearchForLivingTarget();

		//if (isZombie && !IsEatableTargetDetected)
		if (isZombie)
			SearchForEatableTarget();
	}

	#region timed target search types
	private void SearchForLivingTarget()
	{
		detectTargetTimer -= Time.deltaTime;
		if (detectTargetTimer > 0) return;
		detectTargetTimer = detectTargetCooldown;

		//skip looking if target already found
		if (IsTargetDetected && DetectedTarget.StatsHandler.IsDead)
		{
			IsTargetDetected = false;
			DetectedTarget = null;
		}
		else if (IsTargetDetected)
		{
			Vector3 dirToTarget = (DetectedTarget.Collider.bounds.center - rayViewPoint.transform.position).normalized;
			if (TargetInVisionConeAngle(dirToTarget) && TargetInLineOfSight(dirToTarget, lineOfSightMask, DetectedTarget.Collider)) return;

			InvestigateLastSeenEnemyPosition(DetectedTarget.Transform.position);
			IsTargetDetected = false;
			DetectedTarget = null;
		}
		else
		{
			DetectedTarget = SearchForClosestTarget(SearchType.alive);
		}
	}
	private void SearchForEatableTarget()
	{
		detectEatableTargetTimer -= Time.deltaTime;
		if (detectEatableTargetTimer > 0) return;
		detectEatableTargetTimer = detectEatableTargetCooldown;

		TargetData closestTarget = SearchForClosestTarget(SearchType.eatable);

		if (closestTarget != null)
		{
			EatableTarget = closestTarget;
			IsEatableTargetDetected = true;
		}
		else
		{
			EatableTarget = null;
			IsEatableTargetDetected = false;
		}
	}
	#endregion

	/// <summary>
	/// base search method, returns closest valid target after line of sight and filter checks
	/// </summary>
	private TargetData SearchForClosestTarget(SearchType searchType)
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

			if (!FilterSearch(stats, searchType)) continue;

			Vector3 dirToTarget = (collider.bounds.center - rayViewPoint.transform.position).normalized;
			if (!TargetInVisionConeAngle(dirToTarget)) continue;

			if (!TargetInLineOfSight(dirToTarget, lineOfSightMask, collider)) continue;

			float sqrDistance = (stats.transform.position - transform.position).sqrMagnitude;

			if (sqrDistance < closestSqrDistance)
			{
				closestSqrDistance = sqrDistance;
				closestTarget = new TargetData(stats, collider, stats.transform);
			}
		}

		return closestTarget;
	}

	#region search type filter and vision checks
	private bool FilterSearch(StatsHandler target, SearchType searchType)
	{
		if (target == null) return false;
		if (target.Team != NPCSpawner.Teams.FreeFighter && target.Team == StateMachine.StatsHandler.Team) return false;

		if (searchType == SearchType.alive)
		{
			if (!target.IsDead)
				return true;
			else
				return false;
		}
		else if (searchType == SearchType.eatable)
		{
			if (target.EnableZombification && target.IsDead)
				return true;
			else
				return false;
		}
		else
		{
			Debug.LogError($"SearchType: {searchType} not set up, add logic for it, using default alive search");

			if (!target.IsDead)
				return true;
			else
				return false;
		}
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

	/// <summary>
	/// things that should trigger alert mode:
	/// getting attacked by something or finding target to attack
	/// hearing a sound (could specify sounds, or filter sounds made by player (if not zombie) or npcs on same team)
	/// </summary>

	#region npc alert mode triggers
	private void InvestigateLastSeenEnemyPosition(Vector3 position)
	{
		EnableAlertMode();

		StateMachine.locationToInvestigate = position;
		StateMachine.SwitchState(new NPCInvestigateState(StateMachine));
	}
	public void InvestigateSound(Vector3 position)
	{
		EnableAlertMode();

		if (InHigherPriorityState()) return;

		StateMachine.locationToInvestigate = position;
		StateMachine.SwitchState(new NPCInvestigateState(StateMachine));
	}

	private bool InHigherPriorityState()
	{
		State state = StateMachine.CurrentState;

		if (state is NPCRangedAttackState || state is NPCMeleeAttackState || state is NPCChaseState)
			return true;
		else
			return false;
	}
	#endregion

	#region npc alert mode + coroutine
	private void EnableAlertMode()
	{
		if (alertModeCoroutine != null)
			StopCoroutine(alertModeCoroutine);

		alertModeCoroutine = StartCoroutine(AlertModeCoroutine());
	}

	private IEnumerator AlertModeCoroutine()
	{
		isInAlertMode = true;

		viewAngle *= ViewAngleMultiplier;
		viewDistance *= ViewDistanceMultiplier;
		yield return new WaitForSeconds(HighAlertDuration);

		viewAngle = baseViewAngle;
		viewDistance = baseviewDistance;
		isInAlertMode = false;
	}
	#endregion

	private void OnDrawGizmos()
	{
		//draw vision cone for debugging
		if (!showGizmos) return;

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

public class TargetData
{
	public StatsHandler StatsHandler { get; private set; }
	public Collider Collider { get; private set; }
	public Transform Transform { get; private set; }

	public TargetData(StatsHandler statsHandler, Collider collider, Transform transform)
	{
		StatsHandler = statsHandler;
		Collider = collider;
		Transform = transform;
	}
}
