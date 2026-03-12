#if UNITY_EDITOR
using System.Collections;
using Game.Core;
using Game.MyNPC;
using UnityEditor;
using UnityEngine;

public class NpcPerception : MonoBehaviour
{
	public NPCStateMachine StateMachine { get; private set; }

	#region Settings
	[Header("General Settings")]
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
	[SerializeField, ReadOnly] public bool isTargetDetected;
	[SerializeField, ReadOnly] public GameObject DetectedTarget;

	[SerializeField, ReadOnly] public bool isEatableTargetDetected;
	[SerializeField, ReadOnly] public GameObject EatableTarget;
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
		SearchForClosestTarget();
		#endregion

		#region search for eatable targets
		if (isZombie)
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
		GameObject closestTarget = GetClosestTargetInCone();

		if (closestTarget != null)
		{
			DetectedTarget = closestTarget;
			isTargetDetected = true;
		}
		else
		{
			GameObject recordedTarget = null;
			if (DetectedTarget != null)
				recordedTarget = DetectedTarget;

			DetectedTarget = null;
			isTargetDetected = false;

			if (recordedTarget != null)
				InvestigateLastSeenEnemyPosition(recordedTarget.transform.position);
		}
		#endregion
	}

	private GameObject GetClosestTargetInCone()
	{
		#region summary
		/// <summary>
		/// Checks all colliders within view distance and filters
		/// only those inside the vision cone.
		/// Returns the nearest valid target found.
		/// </summary>
		#endregion

		#region GetClosestTarget
		float closestDistance = viewDistance;
		GameObject closestTarget = null;

		for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits); i++)
		{
			var hit = ColliderHits[i];

			if (!StateMachine.TargetTags.Contains(hit.tag) || hit.gameObject == gameObject)
				continue;

			Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
			float angle = Vector3.Angle(transform.forward, dirToTarget);

			// Cone check
			if (angle > viewAngle * 0.5f) // Half-angle used because Vector3.Angle measures from center, not edges
				continue;

			float distance = Vector3.Distance(transform.position, hit.transform.position);

			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestTarget = hit.gameObject;
			}
		}

		return closestTarget;
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
		GameObject closestTarget = GetEatableTargetInCone();

		if (closestTarget != null)
		{
			EatableTarget = closestTarget;
			isEatableTargetDetected = true;
		}
		else
		{
			EatableTarget = null;
			isEatableTargetDetected = false;
		}
		#endregion
	}
	private GameObject GetEatableTargetInCone()
	{
		#region summary
		/// <summary>
		/// Checks all colliders within view distance and filters
		/// only those inside the vision cone.
		/// Returns the nearest valid target found.
		/// </summary>
		#endregion

		#region get closest dead and eatable target
		float closestDistance = viewDistance;
		GameObject closestTarget = null;

		for (int i = 0; i < Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits); i++)
		{
			var hit = ColliderHits[i];

			if (!hit.CompareTag("Dead") || hit.gameObject == gameObject)
				continue;

			Vector3 dirToTarget = (hit.transform.position - transform.position).normalized;
			float angle = Vector3.Angle(transform.forward, dirToTarget);

			// Cone check
			if (angle > viewAngle * 0.5f) // Half-angle used because Vector3.Angle measures from center, not edges
				continue;

			float distance = Vector3.Distance(transform.position, hit.transform.position);

			if (distance < closestDistance)
			{
				closestDistance = distance;
				closestTarget = hit.gameObject;
			}
		}

		return closestTarget;
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

		Color finalColor = isTargetDetected ? detectedColor : normalColor;
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
