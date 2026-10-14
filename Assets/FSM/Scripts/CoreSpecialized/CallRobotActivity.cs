using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace com.lineact.lit.FSM
{
    [CreateAssetMenu(menuName = "LIT/FSM/Activity/CallRobotActivity")]
    public class CallRobotActivity : Activity
    {
        public float Speed;

        public override void Enter(BaseStateMachine stateMachine)
        {
            var conf = stateMachine.GetComponent<CallRobotConfig>();
            var agent = stateMachine.GetComponent<NavMeshAgent>();

            agent.speed = Speed;
            agent.SetDestination(conf.TargetPoint);

            conf.ResetCall();
        }

        public override void Execute(BaseStateMachine stateMachine)
        {
            // Le NavMeshAgent déplace le robot
        }

        public override void Exit(BaseStateMachine stateMachine)
        {

        }
    }
}