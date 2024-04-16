using UnityEngine;

namespace AI.Turret.Turret_States
{
    public class TurretShootingState : TurretBaseState
    {
        public TurretShootingState(AIPerception perception) : base(perception) { }
        public override TurretStates StateKey => TurretStates.Shooting;

        public override TurretStates GetNextState()
        {
            return Perception.HasTarget ? StateKey : TurretStates.Idle;
        }

        public override void UpdateState()
        {
            var nearestTarget = null as Unit;
            var nearestDistance = float.MaxValue;
            foreach (var target in Perception.VisibleUnits)
            {
                if(!target)
                    continue;
                
                var distance = Vector3.Distance(Perception.transform.position, target.transform.position);
                if (!(distance < nearestDistance)) 
                    continue;
                
                nearestDistance = distance;
                nearestTarget = target;
            }

            if (nearestTarget)
                Perception.GetComponent<AIAttack>()?.Queue(nearestTarget);
        }
    }
}