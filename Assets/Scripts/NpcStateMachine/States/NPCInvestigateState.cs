using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NpcBaseMovementState
{
	public NPCInvestigateState(NPCStateMachine stateMachine) : base(stateMachine) { }

	public override void Enter()
	{
		stateMachine.HasInvestigatedLocation = false;
		stateMachine.HasLocationToInvestigate = true;
		MoveToNewDestination(NpcMoveType.moveToInvestigate);
	}

	public override void Exit()
	{
		stateMachine.HasInvestigatedLocation = true;
		stateMachine.HasLocationToInvestigate = false;
	}

	public override void Tick(float deltaTime)
	{
		if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

		#region State Transitions
		// ----------- Investigate to Ranged Attack -------------

		if (stateMachine.TargetInShootingRange && stateMachine.HasEquippedRangedWeapon && stateMachine.EnableRangedAttack)
		{
			stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
			return;
		}

		// ----------- Investigate to Melee Attack -------------

		if (stateMachine.TargetInMeleeRange && stateMachine.HasEquippedMeleeWeapon && stateMachine.EnableMeleeAttack)
		{
			stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
			return;
		}

		// ----------- Investigate to Chase -------------

		if (stateMachine.NpcPerception.IsTargetDetected)
		{
			stateMachine.SwitchState(new NPCChaseState(stateMachine));
			return;
		}

		// ----------- Investigate to idle -------------

		if (HasReachedDestination())
		{
			stateMachine.SwitchState(new NPCIdleState(stateMachine));
			return;
		}
		#endregion

		if (stateMachine.HasLocationToInvestigate && !stateMachine.HasInvestigatedLocation)
		{
			stateMachine.Agent.SetDestination(stateMachine.locationToInvestigate);
			return;
		}
	}
}
