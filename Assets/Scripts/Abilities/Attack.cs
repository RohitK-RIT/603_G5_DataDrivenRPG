using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;
    public float atkRange = 50f;
    public float accuracy = 50f;

    Unit target;

    //Added by Ty
    LineRenderer Line;

    private void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = false;
    }

    public override void Execute()
    {
        if (target)
        {
            target.OnKilled -= Dequeue;

            Vector3 origin = transform.position;
            origin.y += 0.5f;
            Vector3 dest = target.transform.position;
            dest.y += 0.5f;

            Line.SetPosition(0, origin);
            Line.startColor = Color.red;
            Line.endColor = Color.red;

            // Raycast towards target; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
            if (Physics.Raycast(origin, dest - origin, out RaycastHit hit, atkRange, ~(1 << 6))) // ignore other friendly units
            {
                Line.SetPosition(1, hit.point);
                if (hit.collider.TryGetComponent(out Unit u))
                {
                

                    //now factor in accuracy
                    float hitRoll = Random.RandomRange(0, 1f);
                    float hitChance = accuracy;
                    Debug.Log(hitRoll);

                    if (hitRoll < accuracy)
                    {
                        u.TakeDamage(atkDamage);
                        Line.startColor = Color.green;
                        Line.endColor = Color.green;
                    }

                 
                }
            }
            else
            {
                Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
            }
            StartCoroutine(shootLine());
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
            target.OnKilled += Dequeue;
            base.Queue();
        });
    }

    void Dequeue(Unit u)
    {
        if (base.Cancel())
            target = null;
    }
}
