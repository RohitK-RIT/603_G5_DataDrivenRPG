using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;

    Unit target;

    public override void Execute()
    {
        if (target)
            target.TakeDamage(atkDamage);

        base.Execute();
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(Hostility.Hostile, (Unit unit) => { target = unit; });
        base.Queue();
    }
}
