using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;
    public float atkRange = 50f;

    Unit target;

    //Added by Ty
    LineRenderer Line;
    Transform LineOrigin;

    private void Start()
    {
        Line = GetComponent<LineRenderer>();
        LineOrigin = GetComponent<Transform>();
        Line.enabled = false;
    }

    private void Update()
    {
        Line.SetPosition(0, LineOrigin.position);
    }

    public override void Execute()
    {
        if (target)
        {
            // Raycast towards target; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out RaycastHit hit, atkRange, ~(1 << 6)))
            {
                if (hit.collider.TryGetComponent(out Unit u))
                {
                    u.TakeDamage(atkDamage);
                    Line.SetPosition(1, hit.collider.transform.position);
                    StartCoroutine(shootLine());
                }
            }
        }
        base.Execute();
    }

    IEnumerator shootLine()
    {
        Line.enabled = true;
        yield return new WaitForSeconds(.5f);
        Line.enabled = false;
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(Hostility.Hostile, (Unit unit) => 
        {
            target = unit;
            base.Queue();
        });
        
    }
}
