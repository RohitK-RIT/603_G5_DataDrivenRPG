using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : UnitAbility
{
    public float atkDamage = 25f;
    public float atkRange = 50f;
    public float accuracy = 50f;
    [SerializeField] Texture2D SelectionCursor;

    Unit target;

    //Added by Ty
    LineRenderer Line;

    private void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (target)
        {
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
                if (hit.collider.gameObject == target.gameObject)
                {
                    Line.startColor = Color.green;
                    Line.endColor = Color.green;
                }
            }
            else
            {
                Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
            }
        }
    }

    public override void Execute()
    {
        if (target)
        {
            target.OnKilled -= Cancel;

            Vector3 origin = transform.position;
            origin.y += 0.5f;
            Vector3 dest = target.transform.position;
            dest.y += 0.5f;

            Line.SetPosition(0, origin);
            Line.startColor = Color.red;
            Line.endColor = Color.red;
            Line.widthMultiplier = 0.75f;

            // Raycast towards target; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
            if (Physics.Raycast(origin, dest - origin, out RaycastHit hit, atkRange, ~(1 << 6))) // ignore other friendly units
            {
                Line.SetPosition(1, hit.point);
                if (hit.collider.TryGetComponent(out Unit u))
                {
                    u.TakeDamage(atkDamage);
                    if (u == target)
                    {
                        Line.startColor = Color.green;
                        Line.endColor = Color.green;
                    }
                }
            }
            else
            {
                Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
            }
            target = null;
            StartCoroutine(shootLine());
        }
        base.Execute();
    }

    IEnumerator shootLine()
    {
        yield return new WaitForSeconds(.5f);
        Line.enabled = false;
        Line.widthMultiplier = 0.2f;
    }

    public override void Queue()
    {
        SelectionManager.RequestCastUnit(SelectionCursor, Hostility.Hostile, (Unit unit) =>
        {
            Line.enabled = true;
            if (target) target.OnKilled -= Cancel;
            target = unit;
            target.OnKilled += Cancel;
            base.Queue();
        });
    }

    void Cancel(Unit t)
    {
        if (base.Cancel())
        {
            Debug.Log(t.unitName + " -- " + target.unitName);
            Line.enabled = false;
            target = null;
        }
    }
}
