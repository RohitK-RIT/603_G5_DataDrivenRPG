using System.Collections;
using AI;
using UnityEngine;

public class AIAttack : UnitAbility
{
    public float atkDamage = 25f;
    public float accuracy = 50f;
    [SerializeField] Texture2D SelectionCursor;

    private float atkRange = float.MinValue;

    protected Unit target;

    //Added by Ty
    protected LineRenderer Line;

    protected override void Start()
    {
        Line = GetComponent<LineRenderer>();
        Line.enabled = false;
        atkRange = GetComponent<AIPerception>().SightDistance;
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
        if (!target)
            return;

        target.OnKilled -= Cancel;

        Vector3 origin = transform.position;
        origin.y += 0.5f;
        Vector3 dest = target.transform.position;
        dest.y += 0.5f;

        Line.SetPosition(0, origin);
        Line.startColor = Color.red;
        Line.endColor = Color.red;
        Line.widthMultiplier = 0.75f;

        Line.SetPosition(1, target.transform.position);

        if (!GetComponent<AIPerception>().VisibleUnits.Contains(target))
            return;

        //now factor in accuracy
        float hitRoll = Random.Range(0, 1f);
        float hitChance = accuracy;
        if (hitRoll < accuracy)
        {
            target.TakeDamage(atkDamage);
            Line.startColor = Color.green;
            Line.endColor = Color.green;
        }

        target = null;
        StartCoroutine(shootLine());
        
        base.Execute();
    }

    IEnumerator shootLine()
    {
        yield return new WaitForSeconds(.5f);
        Line.enabled = false;
        Line.widthMultiplier = 0.2f;
    }

    public void Queue(Unit target)
    {
        Line.enabled = true;
        this.target = target;
        target.OnKilled += Cancel;
        base.Queue();
    }

    protected void Cancel(Unit t)
    {
        if (base.Cancel())
        {
            Debug.Log(t.unitName + " -- " + target.unitName);
            Line.enabled = false;
            target = null;
        }
    }
}