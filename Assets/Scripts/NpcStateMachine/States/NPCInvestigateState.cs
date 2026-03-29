using Game.MyNPC;
using UnityEngine;

public class NPCInvestigateState : NpcBaseMovementState
{
	private bool glanceStarted;
	private bool glanceDone;
	private float glancingDelay;

	public NPCInvestigateState(NPCStateMachine stateMachine) : base(stateMachine) { }

	public override void Enter()
	{
		stateMachine.HasInvestigatedLocation = false;
		stateMachine.HasLocationToInvestigate = true;

		glanceStarted = false;
		glanceDone = false;
		glancingDelay = 2f;
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

		// ----------- Investigate to idle after simulated glancing in both directions -------------

		if (HasReachedDestination())
		{
			if (!glanceStarted)
			{
				glanceStarted = true;
				stateMachine.NpcPerception.SimulateNpcGlancing(glancingDelay);
			}
			else
			{
				glancingDelay -= deltaTime;

				if (glancingDelay < 0)
					glanceDone = true;
			}

			if (glanceDone)
			{
				stateMachine.SwitchState(new NPCIdleState(stateMachine));
				return;
			}
		}

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
		#endregion
	}
}
