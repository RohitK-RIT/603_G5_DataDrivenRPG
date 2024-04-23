using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EMPGrenade : UnitAbility
{
    public float radius = 5f;
    public float stunTime = 4f;
    public Texture2D selectionCursor;

    Vector3 targetPos;

    EMPGrenade()
    {
        abilityName = "EMP Grenade";
        description = $"Throw a grenade at the target location. All enemies caught inside the blast are stunned for {stunTime} seconds.";
    }

    public override void Execute()
    {
        // TODO: implement enemy stun. 
    }

    public override void Queue()
    {
        SelectionManager.RequestCastPosition(selectionCursor, radius, (Vector3 v) =>
        {
            targetPos = v;
            base.Queue();
        });
    }
}
