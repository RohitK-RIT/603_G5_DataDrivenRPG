using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heal : UnitAbility
{
    Unit healTarget;
    public float healAmt = 200f;
    public Texture2D selectionCursor;

    public override void Execute()
    {
        if (!healTarget) return;

        healTarget.Heal(healAmt);
        healTarget = null;

        base.Execute();
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(selectionCursor, Hostility.Friendly, (Unit unit) =>
        {
            if (unit == thisUnit)
            {
                HUDController.ShowError($"The {thisUnit.unitName} cannot heal themselves. Choose another target.");
                return;
            }
            if (healTarget) healTarget.OnKilled -= Cancel;
            healTarget = unit;
            healTarget.OnKilled += Cancel;
            base.Queue();
        });
    }

    void Cancel(Unit t)
    {
        if (base.Cancel())
        {
            healTarget = null;
        }
    }
}
