using AI.Turret.Turret_States;
using StateMachine;
using UnityEngine;

namespace AI.Turret
{
    public class TurretStateMachine : StateManager<TurretStates>
    {
        [SerializeField] private AIPerception perception;

        protected override void Start()
        {
            States.Add(TurretStates.Idle, new TurretIdleState(perception));
            States.Add(TurretStates.Shooting, new TurretShootingState(perception));

            CurrentState = States[TurretStates.Idle];
            base.Start();
        }
    }
}