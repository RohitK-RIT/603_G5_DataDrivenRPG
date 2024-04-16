using System.Collections;
using UnityEngine;

namespace AI.Turret
{
    public class AIAttack : UnitAbility
    {
        public float atkDamage = 25f;

        Unit target;

        public override void Execute()
        {
            if(!GetComponent<AIPerception>().VisibleUnits.Contains(target))
                return;
            
            if (target)
            {
                target.TakeDamage(atkDamage);
            }

            base.Execute();
        }

        public void Queue(Unit unit)
        {
            target = unit;
            base.Queue();
        }
    }
}