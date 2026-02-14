using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCBrain : MonoBehaviour
{
    #region Path Options
    [Header("Path Options")]
    public bool moveOnStraightPath = true;

    public bool moveOnRandomPath = false;
    public bool StopOccasionally = false;
    #endregion

    #region Patrol Points
    [Header("Patrol Points")]
    public Transform PatrolFollowPoint;
    public Transform RandomFollowPoint;
    #endregion

    #region References
    private NavMeshAgent agent;
    public Animator Animator;
    #endregion

    private void Awake()
    {
        #region GetComponents
        agent = GetComponent<NavMeshAgent>();

        Animator = GetComponent<Animator>();
        #endregion
    }

    private void Update()
    {
        #region  Functions
        AssignDestination();
        UpdtadeAnimator();
        #endregion
    }

    private void OnValidate()
    {
        #region Single Option Enforcement
        if (moveOnStraightPath) moveOnRandomPath = false;
        if (moveOnRandomPath) moveOnStraightPath = false;
        #endregion
    }

    private void AssignDestination()
    {
        #region Destination Assignment
        if (moveOnStraightPath && PatrolFollowPoint != null)
        {
            agent.SetDestination(PatrolFollowPoint.position);
            return;
        }
        else if (moveOnRandomPath && RandomFollowPoint != null)
        {
            agent.SetDestination(RandomFollowPoint.position);
            return;
        }

        Debug.LogWarning($"{name} has no valid patrol configuration!");
        #endregion
    }

    private void UpdtadeAnimator()
    {
        Animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
