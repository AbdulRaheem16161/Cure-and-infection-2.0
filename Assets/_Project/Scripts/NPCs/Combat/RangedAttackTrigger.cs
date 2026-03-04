using UnityEditor;
using UnityEngine;
using Game.MyNPC;
using System.Diagnostics.Contracts;
using NUnit.Framework.Internal.Filters;

public class RangedAttackTrigger : MonoBehaviour
{
    public NPCStateMachine stateMachine;
    public Color GizmozColor = Color.blue;
    public Color DetectedGizmozColor = Color.red;
    public bool ShowGizmoz;

    public float Range = 5f;
    public float distance;

    private void Update()
    {
        if (stateMachine.StatsHandler.IsDead) return;

        Range = stateMachine.EquipmentHandler.rangedWeaponInHands.WeaponDefinition.EffectiveRange; // Trigger's range equal weapon

        if (stateMachine.DetectionRadius.DetectedTarget == null)
        {
            stateMachine.OpponentInRangedAttackRange = false;
            return;
        }   

        CheckIfOpponentInRange();
    }

    private void CheckIfOpponentInRange()
    {
        if (stateMachine.DetectionRadius.DetectedTarget == null)
        {
            stateMachine.OpponentInRangedAttackRange = false;
            return;
        }

        Vector3 directionToTarget = stateMachine.DetectionRadius.DetectedTarget.transform.position - transform.position;
        distance = directionToTarget.magnitude;

        if (distance <= Range)
        {
            stateMachine.OpponentInRangedAttackRange = true;
        }
        else
        {
            stateMachine.OpponentInRangedAttackRange = false;
        }
    }

    private void OnDrawGizmos()
    {
        #region Draw Range Line

        if (!ShowGizmoz) return;

        if (stateMachine.OpponentInRangedAttackRange)
        {
            Gizmos.color = DetectedGizmozColor;
        }
        else
        {
            Gizmos.color = GizmozColor;
        }

        Vector3 start = transform.position;

        Vector3 end = start + transform.forward * Range;

        // Draw the line
        Gizmos.DrawLine(start, end);

        // Optional: Draw a small sphere at the end for clarity
        Gizmos.DrawSphere(end, 0.1f);
        #endregion
    }
}
