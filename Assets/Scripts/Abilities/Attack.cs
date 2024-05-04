using System;
using System.Collections;
using Core.Managers.Analytics;
using UnityEngine;

public class Attack : UnitAbility
{
    protected float atkDamage = 0f;
    protected float atkRange = 0f;
    protected float accuracy = 0f;
    [SerializeField] Texture2D SelectionCursor;

    Unit target;

    //Added by Ty
    CoverTrigger Cover;
    Vector3 origin;
    Vector3 dest;
    protected LineRenderer Line;

    //equipped weapon (a Scriptable Object)
    //added by Taode
    protected Weapon equippedWeapon;
    int precision;


    protected virtual void Start()
    {
        Line = GetComponent<LineRenderer>();
        Cover = GetComponent<CoverTrigger>();
        equippedWeapon = GetComponent<Unit>().equippedWeapon;
        precision = GetComponent<Unit>().precision;
        Line.enabled = false;

        //added by Taode, Modified by Ty
        //modify the attack action on this unit to account for equipped weapon

        if (equippedWeapon != null)
        {
            atkDamage = equippedWeapon.damage_per_shot;
            atkRange = equippedWeapon.range;
            //Add Precision bonus here
            //1% per Precision point
            accuracy = equippedWeapon.baseAccuracy + (precision / 100f);
            description = equippedWeapon.ParsedDescription();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (target)
        {
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

            Line.SetPosition(0, origin);
            Line.startColor = Color.red;
            Line.endColor = Color.red;

            // Raycast towards target; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
            if (Physics.Raycast(origin, dest - origin, out RaycastHit hit, atkRange, ~(1 << 6))) // ignore other friendly units
            {
                Line.SetPosition(1, hit.point);
                if (hit.collider.gameObject == target.gameObject)
                {
                    Line.startColor = Color.cyan;
                    Line.endColor = Color.cyan;
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

            Line.SetPosition(0, origin);
            Line.startColor = Color.red;
            Line.endColor = Color.red;
            Line.widthMultiplier = 0.5f;

            Ray ray = new Ray(origin, dest - origin);
            RaycastHit[] hits = new RaycastHit[2];
            hits[0].distance = 999999;
            hits[1].distance = 999999;
            int numHits = Physics.RaycastNonAlloc(ray, hits, atkRange, ~(1 << 6), QueryTriggerInteraction.Ignore);

            if (numHits > 0)
            {
                Array.Sort(hits, (RaycastHit x, RaycastHit y) => x.distance.CompareTo(y.distance));

                if (hits[0].collider.gameObject.layer == 7)
                {
                    if (hits[0].collider.TryGetComponent(out Unit u))
                    {
                        Line.SetPosition(1, hits[0].point);
                        if (u == target)
                        {
                            Line.startColor = Color.green;
                            Line.endColor = Color.green;
                            new WeaponUsedEvent(u.equippedWeapon).Raise();
                            u.TakeDamage(atkDamage);
                        }
                    }
                }
                else if (hits[0].collider.gameObject.layer == 8 && hits[1].collider.gameObject.layer == 7)
                {
                    if (hits[1].collider.TryGetComponent(out Unit u))
                    {
                        Line.SetPosition(1, hits[1].point);
                        if (u == target)
                        {
                            Line.startColor = Color.green;
                            Line.endColor = Color.green;
                            new WeaponUsedEvent(u.equippedWeapon).Raise();
                            u.TakeDamage(atkDamage / 2);
                        }
                    }
                }
                else
                {
                    Line.SetPosition(1, origin + (dest - origin).normalized * atkRange);
                }
                // Debug.Log(hits[0].collider.gameObject.layer);
                // Debug.Log(hits[1].collider.gameObject.layer);
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
            Line.enabled = false;
            target = null;
        }
    }
}