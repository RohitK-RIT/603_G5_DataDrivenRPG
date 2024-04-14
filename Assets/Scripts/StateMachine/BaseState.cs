using System;
using UnityEngine;

namespace StateMachine
{
    public abstract class BaseState<TState> where TState : Enum
    {
        public abstract TState StateKey { get; }

        public virtual void EnterState() { }

        public virtual void ExitState() { }

        public virtual void UpdateState() { }

        public virtual void FixedUpdateState() { }

        public abstract TState GetNextState();

        public virtual void OnTriggerEnter(Collider2D other) { }

        public virtual void OnTriggerStay(Collider2D other) { }

        public virtual void OnTriggerExit(Collider2D other) { }
    }
}