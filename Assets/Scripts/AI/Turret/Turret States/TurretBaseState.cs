using StateMachine;

namespace AI.Turret.Turret_States
{
    public abstract class TurretBaseState : BaseState<TurretStates>
    {
        protected readonly AIPerception Perception;
        public TurretBaseState(AIPerception perception)
        {
            Perception = perception;
        }
    }
}