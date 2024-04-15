namespace AI.Turret.Turret_States
{
    public class TurretIdleState : TurretBaseState
    {
        public TurretIdleState(AIPerception perception) : base(perception) { }
        public override TurretStates StateKey => TurretStates.Idle;

        public override TurretStates GetNextState()
        {
            return Perception.HasTarget ? TurretStates.Shooting : StateKey;
        }
    }
}