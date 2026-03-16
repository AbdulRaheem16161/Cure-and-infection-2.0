#if UNITY_EDITOR
using Game.Core;
using Game.MyNPC;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.Image;

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
	private List<StatsHandler> visibleTargets = new();
	public bool IsTargetDetected { get; private set; }
	public StatsHandler DetectedTarget { get; private set; }

	public bool IsEatableTargetDetected { get; private set; }
	public StatsHandler EatableTarget { get; private set; }


	#endregion

	#region search types
	public enum SearchType
	{
		alive, eatable
	}
	#endregion

	#region detect target timer
	public float detectTargetCooldown = 0.1f;
	public float detectTargetTimer;
	#endregion

	#region detect eatable target timer
	public float detectEatableTargetCooldown = 0.5f;
	public float detectEatableTargetTimer;
	#endregion

	public void Initialize(NpcDefinition npcDefinition, NPCStateMachine stateMachine)
	{
		if (rayViewPoint == null)
			Debug.LogError("rayViewPoint null, assign empty object where raycasts should start from");

		#region Initialize
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
		#endregion
	}

	private void Update()
	{
		if (StateMachine.StatsHandler.IsDead) return;

		#region search for targets
		//if (!IsTargetDetected)
			SearchForClosestTarget();
		#endregion

		#region search for eatable targets
		//if (isZombie && !IsEatableTargetDetected)
			SearchForEatableTarget();
		#endregion
	}

	private void SearchForClosestTarget()
	{
		#region summary
		/// <summary>
		/// search for targets based on timer, either set new target if found, or investigate position of where one was last seen
		/// </summary>
		#endregion

		#region timer
		detectTargetTimer -= Time.deltaTime;
		if (detectTargetTimer > 0) return;
		detectTargetTimer = detectTargetCooldown;
		#endregion

		#region Update Detected Target
		StatsHandler closestTarget = SearchForClosestTarget(SearchType.alive);

		if (closestTarget != null)
		{
			DetectedTarget = closestTarget;
			IsTargetDetected = true;
		}
		else
		{
			StatsHandler recordedTarget = null;
			if (DetectedTarget != null)
				recordedTarget = DetectedTarget;

			DetectedTarget = null;
			IsTargetDetected = false;

			if (recordedTarget != null)
				InvestigateLastSeenEnemyPosition(recordedTarget.transform.position);
		}
		#endregion
	}

	private void SearchForEatableTarget()
	{
		#region summary
		/// <summary>
		/// search for eatable targets if zombie based on timer, either set new target if found, or set null
		/// </summary>
		#endregion

		#region timer
		detectEatableTargetTimer -= Time.deltaTime;
		if (detectEatableTargetTimer > 0) return;
		detectEatableTargetTimer = detectEatableTargetCooldown;
		#endregion

		#region Update Eatable Target
		StatsHandler closestTarget = SearchForClosestTarget(SearchType.eatable);

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
		#endregion
	}

	private StatsHandler SearchForClosestTarget(SearchType searchType)
	{
		#region summary
		/// <summary>
		/// Checks all colliders within view distance use filters to filter targets
		/// filter again via line of sight using raycast, sort visible targets by closest and return
		/// </summary>
		#endregion

		#region search for closest target
		visibleTargets.Clear();
		float closestDistance = viewDistance;
		StatsHandler closestTarget = null;
		LayerMask mask = LayerMask.GetMask("Environment", "Characters"); // only layers that could block

		//grab all targets in view distance
		for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits, mask); i++)
		{
			Collider collider = ColliderHits[i];
			GameObject go = collider.gameObject;

			//filter self, team mates and filter type
			if (gameObject == go) continue;
			StatsHandler stats = go.GetComponent<StatsHandler>();
			if (!FilterSearch(stats, searchType)) continue;

			//filter for targets in vision cone
			Vector3 dirToTarget = (collider.transform.position - transform.position).normalized;
			float dot = Vector3.Dot(transform.forward, dirToTarget);
			float cosHalfView = Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad);
			if (dot < cosHalfView) continue;

			//check if target is visible with raycast
			for (int j = 0; j < Physics.RaycastNonAlloc(
				rayViewPoint.transform.position, dirToTarget, RaycastHits, viewDistance, mask, QueryTriggerInteraction.Ignore); j++)
			{
				RaycastHit hit = RaycastHits[j];

				if (hit.collider.gameObject == gameObject) continue; //ignore self
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Environment")) break;
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Characters") && collider.gameObject == hit.collider.gameObject)
				{
					visibleTargets.Add(stats);
					break;
				}
			}
		}

		for (int i = 0; i < visibleTargets.Count; i++)
		{
			StatsHandler target = visibleTargets[i];
			float distance = Vector3.Distance(transform.position, target.transform.position);

			if (distance < closestDistance) //track closest
			{
				closestDistance = distance;
				closestTarget = target;
			}
		}

		return closestTarget;
		#endregion
	}

	private bool FilterSearch(StatsHandler target, SearchType searchType)
	{
		#region filter logic, true = pass, false = fail
		if (target == null) return false;
		if (target.Team != NPCSpawner.Teams.FreeFighter)
			if (target.Team == StateMachine.StatsHandler.Team) return false;

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
		#endregion
	}

	private void OnDisable()
	{
		#region OnDisable
		showGizmos = false;
		#endregion
	}

	/// <summary>
	/// things that should trigger alert mode:
	/// getting attacked by something or finding target to attack
	/// hearing a sound (could specify sounds, or filter sounds made by player (if not zombie) or npcs on same team)
	/// </summary>

	private void InvestigateLastSeenEnemyPosition(Vector3 position)
	{
		#region summary
		/// <summary>
		/// if detected enemy no longer detected and no other enemies are detected, investigate last seen enemies position
		/// </summary>
		#endregion

		#region enable alert mode
		EnableAlertMode();
		#endregion

		#region investigate position
		StateMachine.locationToInvestigate = position;
		StateMachine.SwitchState(new NPCInvestigateState(StateMachine));
		#endregion
	}
	public void InvestigateSound(Vector3 position)
	{
		#region summary
		/// <summary>
		/// enable alert mode, if check pass, set position to investigate and enter investigate state
		/// </summary>
		#endregion

		#region enable alert mode
		EnableAlertMode();
		#endregion

		#region set position to investigate
		if (InHigherPriorityState()) return;

		StateMachine.locationToInvestigate = position;
		StateMachine.SwitchState(new NPCInvestigateState(StateMachine));
		#endregion
	}

	private bool InHigherPriorityState()
	{
		#region ignore higher priority states
		State state = StateMachine.CurrentState;

		if (state is NPCRangedAttackState || state is NPCMeleeAttackState || state is NPCChaseState)
		{
			Debug.LogError("in higher priority");
			return true;
		}
		else
		{
			Debug.LogError("in lower priority");
			return false;
		}
		#endregion
	}

	private void EnableAlertMode()
	{
		#region Trigger Alert Mode Coroutine or reset
		if (alertModeCoroutine != null)
			StopCoroutine(alertModeCoroutine);

		alertModeCoroutine = StartCoroutine(AlertModeCoroutine());
		#endregion
	}

	private IEnumerator AlertModeCoroutine()
	{
		#region Start Alert mode
		isInAlertMode = true;

		viewAngle *= ViewAngleMultiplier;
		viewDistance *= ViewDistanceMultiplier;
		yield return new WaitForSeconds(HighAlertDuration);

		viewAngle = baseViewAngle;
		viewDistance = baseviewDistance;
		isInAlertMode = false;
		#endregion
	}

	private void OnDrawGizmos()
	{
		#region OnDrawGizmos
		#region summary
		/// <summary>
		/// Draws the vision cone in the editor to visualize
		/// NPC awareness and detection state.
		/// </summary>
		#endregion

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
		#endregion
	}
}
#endif
