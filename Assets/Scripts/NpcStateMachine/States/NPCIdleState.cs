using Game.Core;
using UnityEngine;

namespace Game.MyNPC
{
    public class NPCIdleState : NPCBaseState
    {
        private float WaitBeforeFreeMove;
        private float timer;

        public NPCIdleState(NPCStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            WaitBeforeFreeMove = Random.Range(stateMachine.NpcDefinition.MinIdleTime, stateMachine.NpcDefinition.MaxIdleTime);
            timer = 0f;
            stateMachine.Animator.SetFloat("Speed", 0f);
            stateMachine.Agent.speed = 0f;
        }

        public override void Exit()
        {
   
        }

        public override void Tick(float deltaTime)
        {
			if (stateMachine.StatsHandler.LifeState == NpcDefinition.LifeState.dead) return;

			// ----------- Idle to Free Move -------------
			timer += deltaTime;
			stateMachine.Animator.SetFloat("Speed", stateMachine.CurrentSpeed);

			// Wait until random time is over
			if (timer >= WaitBeforeFreeMove && stateMachine.EnableMovement)
			{
				stateMachine.SwitchState(new NPCMoveState(stateMachine));
				return;
			}

			#region State Transitions
			// ----------- Idle to Ranged Attack -------------
			if (stateMachine.TargetInShootingRange && stateMachine.EnableRangedAttack)
			{
				stateMachine.SwitchState(new NPCRangedAttackState(stateMachine));
				return;
			}

			// ----------- Idle to Melee Attack -------------
			if (stateMachine.TargetInMeleeRange && stateMachine.EnableMeleeAttack)
			{
				stateMachine.SwitchState(new NPCMeleeAttackState(stateMachine));
				return;
			}

			// ----------- Idle to Chase -------------
			if (stateMachine.NpcPerception.IsTargetDetected && stateMachine.EnableChase)
			{
				stateMachine.SwitchState(new NPCChaseState(stateMachine));
				return;
			}

			// ----------- Free Move to Eat Corpse -------------
			if (stateMachine.NpcPerception.IsEatableTargetDetected && stateMachine.EnableEatCorpseState)
			{
				stateMachine.SwitchState(new NPCEatCorpseState(stateMachine));
				return;
			}

			// ----------- Chase to Flee -------------
			if (stateMachine.EnableFlee && stateMachine.TargetInFleeRange)
			{
				stateMachine.SwitchState(new NpcFleeState(stateMachine));
				return;
			}
            #endregion
        }
    }
}
