using System.Collections;
using UnityEngine;

namespace AI.Turret
{
    public class AIAttack : UnitAbility
    {
        public float atkDamage = 25f;

        private LineRenderer Line => _line ??= GetComponent<LineRenderer>();

        private Unit _target;
        private LineRenderer _line;


        private void Start()
        {
            Line.enabled = false;
        }

        public override void Execute()
        {
            if (!GetComponent<AIPerception>().VisibleUnits.Contains(_target))
                return;

            if (_target)
            {
                _target.TakeDamage(atkDamage);
                Line.SetPosition(1, _target.transform.position);
                StartCoroutine(shootLine());
            }

            base.Execute();
        }

        public void Queue(Unit unit)
        {
            _target = unit;
            base.Queue();
        }

        protected override void Update()
        {
            base.Update();
            Line.SetPosition(0, transform.position);
        }

        IEnumerator shootLine()
        {
            Line.enabled = true;
            yield return new WaitForSeconds(.5f);
            Line.enabled = false;
        }
    }
}