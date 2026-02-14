using Game.MyNPC;
using UnityEngine;

public class MeleeAttackTrigger : MonoBehaviour
{
    private NPCStateMachine stateMachine;

    [ReadOnly] public string TargetTag;
    public string DeadOpponentTag;


    private void Awake()
    {
        stateMachine = GetComponentInParent<NPCStateMachine>();
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("colliders's Tag = " + other.tag);

        if (stateMachine.TargetTags.Contains(other.tag))
        {
            stateMachine.OpponentInMeleeAttackRange = true;
        }
        else if (other.tag == DeadOpponentTag)
        {
            stateMachine.OpponentInMeleeAttackRange = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit" + other.tag);
        if (stateMachine.TargetTags.Contains(other.tag) || other.tag == DeadOpponentTag)
        {
            stateMachine.OpponentInMeleeAttackRange = false;
        }
    }
}
