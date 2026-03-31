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
			if (timer >= WaitBeforeFreeMove)
			{
                stateMachine.Beliefs.Idling = false;
			}
        }
    }
}
