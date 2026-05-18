#if UNITY_EDITOR
using Game.MyNPC;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static EntityDefinition;

[RequireComponent(typeof(NpcBeliefs))]
[RequireComponent(typeof(NPCStateMachine))]
[RequireComponent(typeof(StatsHandler))]
[RequireComponent(typeof(EquipmentHandler))]
public class NpcPerception : MonoBehaviour
{
	public EntityDefinition Definition {  get; private set; }
	public NpcBeliefs Beliefs { get; private set; }
	public NPCStateMachine StateMachine { get; private set; }
	public StatsHandler StatsHandler { get; private set; }
	public EquipmentHandler EquipmentHandler{ get; private set; }

    public GameObject rayViewPoint;

	#region Runtime Vision Values
	[Header("Runtime Vision Values")]
	public float viewAngle;
	public float viewDistance;
	private bool glancing;
	public bool showVision = false;
	#endregion

	#region layer Masks (internal config)
	private LayerMask targetMask;
	private LayerMask lineOfSightMask;
	private LayerMask coverMask;
    private LayerMask interactablesMask;
    #endregion

    #region Runtime Targets
    [Header("Runtime Targets")]
	public TargetData Target {get; private set;}
	public TargetData EatableTarget {get; private set;}
	public TargetData ClosestFleeTarget { get; private set;}

    public enum TargetTrackResult
    {
        valid, invalid, lost
    }
    #endregion

    #region Found Interactables
    [Header("Found Interactables")]
	public List<InteractContext> interactables = new();
    public List<InteractContext> doorsInPath = new();
    #endregion

    private Color normalColor = Color.green;
	private Color detectedColor = Color.red;
	private readonly float colorAlpha = 0.25f;

	private readonly Collider[] ColliderHits = new Collider[100];
	private readonly RaycastHit[] RaycastHits = new RaycastHit[100];

	#region alert mode timer + Coroutine;
	private readonly float alertModeCooldown = 5f;
	private float alertModeTimer;
    public Coroutine alertModeCoroutine;
    #endregion

    #region living target search timer
    private readonly float detectTargetCooldown = 0.1f;
	private float detectTargetTimer;
    #endregion

    #region eatable target search timer
    private readonly float detectEatableTargetCooldown = 0.5f;
	private float detectEatableTargetTimer;
    #endregion

    #region flee target search timer
    private readonly float fleeTargetCooldown = 0.25f;
	private float fleeTargetTimer;
    #endregion

    #region cover object search timer
    private readonly float coverSearchCooldown = 2f;
	private float coverSearchDelay;
    #endregion

    #region interactables search timer
    private readonly float interactablesSearchCooldown = 1f;
    private float interactablesSearchDelay;
    #endregion

    #region awake + Initialize
    private void Awake()
	{
		Beliefs = GetComponent<NpcBeliefs>();
		StateMachine = GetComponent<NPCStateMachine>();
		StatsHandler = GetComponent<StatsHandler>();
		EquipmentHandler = GetComponent<EquipmentHandler>();

		targetMask = LayerMask.GetMask("CharacterDetection");
		lineOfSightMask = LayerMask.GetMask("Environment", "EnvironmentCover", "CharacterDetection");
		coverMask = LayerMask.GetMask("EnvironmentCover");
		interactablesMask = LayerMask.GetMask("Interactable");

		//randomize initial timer to mitigate lag spikes
        detectTargetTimer = Random.Range(0f, detectTargetCooldown);
        detectEatableTargetTimer = Random.Range(0f, detectEatableTargetCooldown);
        fleeTargetTimer = Random.Range(0f, fleeTargetCooldown);
        coverSearchDelay = Random.Range(0f, coverSearchCooldown);
        interactablesSearchDelay = Random.Range(0f, interactablesSearchCooldown);

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
		SearchForCoverObject();
		SearchForInteractables();
	}

	/// <summary>
	/// things that should trigger investigations
	/// getting attacked by something or moosing target to attack
	/// hearing a sound (could specify sounds, or filter sounds made by player (if not zombie) or npcs on same team)
	/// </summary>

	#region Npc Investigation Triggers
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

	#region Npc Alert Mode Timer + Updating Vision State
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

	#region Npc Glance Simulation (nice to have head glance animation)
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

	#region Timer Searches For Targets (LivingTarget, EatableTarget, FleeTarget)
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
            (Target, trackResult) = TrackTarget(Target, LifeState.alive, LifeState.zombified);

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

    #region Timer Searches For Utility Things (CoverObject, Interactables)
    private void SearchForCoverObject()
	{
		if (!ShouldUseCover()) return;

		coverSearchDelay -= Time.deltaTime;
		if (coverSearchDelay > 0) return;
		coverSearchDelay = coverSearchCooldown;

		if (FindValidCover(out Vector3? coverMovePosition))
		{
			Beliefs.UpdateCoverPosition(coverMovePosition);
			return;
		}
	}
    private void SearchForInteractables()
    {
        interactablesSearchDelay -= Time.deltaTime;
        if (interactablesSearchDelay > 0) return;
        interactablesSearchDelay = interactablesSearchCooldown;
		FindInteractables();
    }
    #endregion

    #region Shared Search For Closest Target Method
    /// <summary>
    /// base search method, returns closest valid target after line of sight and filter checks
    /// </summary>
    private TargetData SearchForClosestTarget(LifeState lifeState)
	{
		int count = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits, targetMask);
        float closestSqrDistance = viewDistance * viewDistance;
		TargetData closestTarget = null;

		for (int i = 0; i < count; i++)
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

    #region Handle Target Types Filtering
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
    #endregion

    #region Handle Target Vision Cone/Line of Sight Filtering
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
		int count = Physics.RaycastNonAlloc(rayViewPoint.transform.position, dirToTarget, RaycastHits, viewDistance, mask, QueryTriggerInteraction.Ignore);
		float closestTargetDistance = viewDistance;
		float closestBlockingDistance = viewDistance;

		for (int i = 0; i < count; i++)
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

	#region Handle tracking found targets and loosing them
	private (TargetData, TargetTrackResult) TrackTarget(TargetData trackedTarget, params LifeState[] validStates)
	{
		if (!validStates.Contains(trackedTarget.StatsHandler.LifeState))
			return (null, TargetTrackResult.invalid);

		trackedTarget.UpdateTargetDistance(transform.position);
		Vector3 dirToTarget = (trackedTarget.Collider.bounds.center - rayViewPoint.transform.position).normalized;
		if (TargetInVisionConeAngle(dirToTarget) && TargetInLineOfSight(dirToTarget, lineOfSightMask, trackedTarget.Collider)) 
			return (trackedTarget, TargetTrackResult.valid);

		return (null, TargetTrackResult.lost); //no longer in vision cone or line of sight
	}
	#endregion

	#region Handle looking for valid cover position when requested
	private bool FindValidCover(out Vector3? coverMovePosition)
	{
        int count = Physics.OverlapSphereNonAlloc(transform.position, viewDistance, ColliderHits, coverMask);
        List<CoverObject> validCovers = new();
		coverMovePosition = null;

        for (int i = 0; i < count; i++)
		{
			if (ColliderHits[i].TryGetComponent(out CoverObject cover)) //filter 
				validCovers.Add(cover);
		}

		validCovers = FilterAndSortCovers(Beliefs.Target, validCovers);

		foreach (CoverObject cover in validCovers)
		{
			if (cover.GetClosestPointBehindCover(transform.position, Beliefs.Target.Transform.position, out Vector3? coverPosition))
			{
				coverMovePosition = coverPosition;
				return true;
			}
		}
		return false;
	}

	private List<CoverObject> FilterAndSortCovers(TargetData threat, List<CoverObject> foundCovers)
	{
		for (int i = foundCovers.Count - 1; i >= 0; i--)
		{
			float coverSqrDistanceToSelf = (foundCovers[i].transform.position - transform.position).sqrMagnitude;
			float coverSqrDistanceToThreat = (foundCovers[i].transform.position - threat.Transform.position).sqrMagnitude;

			if (CoverWithinSquaredDistance(Definition.FleeSqrDistance, coverSqrDistanceToThreat) ||
				CoverOutsideEquippedWeaponRange(coverSqrDistanceToSelf))
			{
				foundCovers.RemoveAt(i);
				continue;
			}
		}

		foundCovers.Sort((a, b) =>
		{
			float aSqrDistance = (a.transform.position - transform.position).sqrMagnitude;
			float bSqrDistance = (b.transform.position - transform.position).sqrMagnitude;
			return aSqrDistance.CompareTo(bSqrDistance);
		});

		return foundCovers;
	}

	private bool CoverWithinSquaredDistance(float squaredDistance, float coverSqrDistance)
	{
		return coverSqrDistance + 1 < squaredDistance;
	}
	private bool CoverOutsideEquippedWeaponRange(float coverSqrDistance)
	{
		if (EquipmentHandler.itemInHands.ItemDefinition is WeaponRangedDefinition rangedWeapon)
			return coverSqrDistance > rangedWeapon.EffectiveSqrRange;

		return false;
	}
    #endregion

    #region Should Use Cover Logic Check
    //limits use of cover when target is a non zombified humanoid (ignores animals/zombies basically)
    private bool ShouldUseCover()
    {
        if (Beliefs.Target == null) return false;

        EntityDefinition targetDefinition = Beliefs.Target.StatsHandler.Definition;
        if (targetDefinition is HumanoidDefinition humanoid)
        {
            if (humanoid.Flags.HasFlag(EntityFlags.canBecomeZombie))
                return true;
        }

        return false;
    }
    #endregion

    #region Handle Looking for interactables
	public void FindInteractables()
	{
        int count = Physics.OverlapSphereNonAlloc(transform.position, 25f, ColliderHits, interactablesMask);
        HashSet<IInteractable> newSet = new();
        List<InteractContext> newList = new();

        for (int i = 0; i < count; i++)
        {
            Collider collider = ColliderHits[i];

			if (collider.TryGetComponent(out IInteractable interactable))
            {
                newSet.Add(interactable);
                newList.Add(new InteractContext(interactable, collider, transform.position));
            }
        }

        //remove old ones not in new list
        for (int i = interactables.Count - 1; i >= 0; i--)
        {
            if (!newSet.Contains(interactables[i].interactable))
                interactables.RemoveAt(i);
        }

        //add new ones not in old list
        for (int i = 0; i < newList.Count; i++)
        {
            bool exists = false;

            for (int j = 0; j < interactables.Count; j++)
            {
                if (interactables[j].interactable == newList[i].interactable) //already exists so update
                {
					interactables[j].UpdateDistance(transform.position);
                    exists = true;
                    break;
                }
            }

            if (!exists)
                interactables.Add(newList[i]);
        }
    }
    #endregion

    #region Handle Looking for doors along valid move path
    public void CheckPathForDoors(NavMeshPath path)
    {
        doorsInPath.Clear();

        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            if (i + 1 > path.corners.Length - 1) break; //no more corners to check between
            Vector3 cornerOne = new(path.corners[i].x, path.corners[i].y + 1, path.corners[i].z);
            Vector3 cornerTwo = new(path.corners[i + 1].x, path.corners[i + 1].y + 1, path.corners[i + 1].z);

            int count = Physics.RaycastNonAlloc(cornerOne, cornerTwo, RaycastHits, 100f, interactablesMask, QueryTriggerInteraction.Ignore);

            for (int j = 0; j < count; j++)
            {
                Collider collider = RaycastHits[j].collider;

                if (collider.TryGetComponent(out Door door))
                    doorsInPath.Add(new(door, collider, transform.position));
            }
        }
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmos()
    {
        ShowVisionConeGizmo();
    }

    private void ShowVisionConeGizmo()
    {
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
    #endregion
}
#endif
