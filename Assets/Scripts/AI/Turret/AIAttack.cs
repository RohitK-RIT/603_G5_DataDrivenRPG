using System.Collections;
using System.Linq;
using UnityEngine;

namespace AI.Turret
{
    public class AIAttack : UnitAbility
    {
        //public float atkDamage = 25f;
        //public float accuracy = 50f;

        Unit target;

        //Added by Ty
        protected Weapon equippedWeapon;
        protected LineRenderer Line;
        private float _atkRange;
        private AIPerception _perception;

        private void Start()
        {
            Line = GetComponent<LineRenderer>();
            Line.enabled = false;
            equippedWeapon = GetComponent<Unit>().equippedWeapon;
            _atkRange = GetComponent<AIPerception>().SightDistance;
            _perception = GetComponent<AIPerception>();
        }

        protected override void Update()
        {
            base.Update();

            if (target)
            {
                Vector3 origin = transform.position;
                //origin.y += 0.5f;
                Vector3 dest = target.transform.position;
                //dest.y += 0.5f;

                Line.SetPosition(0, origin);
                Line.startColor = Color.red;
                Line.endColor = Color.red;

                if (_perception.VisibleUnits.Contains(target)) // ignore other friendly units
                {
                    Line.SetPosition(1, target.transform.position);

                    Line.startColor = Color.cyan;
                    Line.endColor = Color.cyan;
                }
                else
                {
                    Line.SetPosition(1, origin + (dest - origin).normalized * _atkRange);
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

                // Check if target still in sight; deal dmg to whatever is hit (which may not be the target if another enemy unit is in the way)
                if (_perception.VisibleUnits.Contains(target))
                {
                    Line.SetPosition(1, target.transform.position);
                    
                    //now factor in accuracy
                    var hitRoll = Random.Range(0, 1f);
                    var hitChance = equippedWeapon.baseAccuracy;
                    if (hitRoll < equippedWeapon.baseAccuracy)
                    {
                        target.TakeDamage(equippedWeapon.damage_per_shot);
                        Line.startColor = Color.green;
                        Line.endColor = Color.green;
                    }
                }
                else
                {
                    Line.SetPosition(1, origin + (dest - origin).normalized * _atkRange);
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

        public void Queue(Unit unit)
        {
            target = unit;
            Queue();
        }

        public override void Queue()
        {
            Line.enabled = true;
            if (target)
                target.OnKilled -= Cancel;
            target.OnKilled += Cancel;
            base.Queue();
        }

        void Cancel(Unit t)
        {
            if (!base.Cancel())
                return;

            Line.enabled = false;
            target = null;
        }
    }
}