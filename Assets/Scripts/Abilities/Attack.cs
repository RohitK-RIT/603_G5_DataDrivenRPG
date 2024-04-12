using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;
    public float atkRange = 50f;

    Unit target;

    public override void Execute()
    {
        if (target)
        {
            // Raycast towards the target. If it hits an enemy unit (which may not necessarily be the target), deal dmg to it.
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out RaycastHit hit, atkRange))
            {
                if (hit.collider.TryGetComponent(out Unit u))
                {
                    if (u.Hostility != GetComponent<Unit>().Hostility)
                        u.TakeDamage(atkDamage);
                }
            }
        }

        base.Execute();
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(Hostility.Hostile, (Unit unit) => { target = unit; });
        base.Queue();
    }
}
