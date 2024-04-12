using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;

    Unit target;
    FieldOfView FOV;

    private void Start()
    {
        FOV = GetComponent<FieldOfView>();
    }

    public override void Execute()
    {
        if (FOV.canSeePlayer)
        {
            if (target)
                target.TakeDamage(atkDamage);
        }
        base.Execute();
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(Hostility.Hostile, (Unit unit) => { target = unit; });
        base.Queue();
    }
}
