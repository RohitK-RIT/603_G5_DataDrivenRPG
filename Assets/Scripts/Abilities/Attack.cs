using System;
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
    CoverTrigger Cover;
    LineRenderer Line;
    Vector3 origin;
    Vector3 dest;
    Ray ray;
    RaycastHit[] hits = new RaycastHit[2];

    private void Start()
    {
        Line = GetComponent<LineRenderer>();
        Cover = GetComponent<CoverTrigger>();
        Line.enabled = false;
    }

    public override void Execute()
    {
        if (target)
        {
            target.OnKilled -= Dequeue;

            if (Cover.isBehindCover == true)
            {
                origin = transform.position;
                origin.y += 0.5f;
                dest = target.transform.position;
                dest.y += 0.5f;
            }
            else
            {
                origin = transform.position;
                dest = target.transform.position;
            }

            ray = new Ray(origin, (dest - origin));
            Line.SetPosition(0, origin);
            Line.material.color = Color.red;

            int numHits = Physics.RaycastNonAlloc(ray, hits, atkRange, ~(1 << 6), QueryTriggerInteraction.Ignore);

            if (numHits > 0)
            {
                Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));

                for (int i = 0; i < numHits; i++)
                {
                    Line.SetPosition(1, hits[i].point);
                    if (hits[0].collider.gameObject.layer == 8 && hits[1].collider.gameObject.layer == 7)
                    {
                        if (hits[i].collider.TryGetComponent(out Unit u))
                        {
                            Line.material.color = Color.green;
                            u.TakeDamage(atkDamage / 2);
                        }
                    }
                    else
                    {
                        if (hits[i].collider.TryGetComponent(out Unit u))
                        {
                            Line.material.color = Color.green;
                            u.TakeDamage(atkDamage);
                        }
                    }
                    Debug.Log(hits[i].collider.gameObject.layer);
                }
            }
            else
            {
                Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
            }












            /*/ Raycast towards target; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
            if (Physics.Raycast(origin, dest - origin, out RaycastHit hit, atkRange, ~(1 << 6))) // ignore other friendly units
            {
                Line.SetPosition(1, hit.point);
                if (hit.collider.TryGetComponent(out Unit u))
                {
                    Line.material.color = Color.green;
                    u.TakeDamage(atkDamage);
                }
            }
            else
            {
                Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
            }
            */
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
