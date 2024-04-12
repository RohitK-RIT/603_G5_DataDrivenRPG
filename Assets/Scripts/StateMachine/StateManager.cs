using System;
using System.Collections.Generic;
using UnityEngine;

namespace AI.StateMachine
{
    public abstract class StateManager<TState> : MonoBehaviour where TState : Enum
    {
        protected readonly Dictionary<TState, BaseState<TState>> States = new();
        protected BaseState<TState> CurrentState;
        [SerializeField] private TState currentStateKey;

        private bool _isTransitioningState;

        private void Start()
        {
            CurrentState.EnterState();
        }

        private void Update()
        {
            var nextStateKey = CurrentState.GetNextState();
            switch (_isTransitioningState)
            {
                case false when nextStateKey.Equals(CurrentState.StateKey):
                    CurrentState.UpdateState();
                    break;
                case false:
                    TransitionToState(nextStateKey);
                    break;
            }

            CurrentState.UpdateState();
        }

        private void TransitionToState(TState nextStateKey)
        {
            _isTransitioningState = true;
            CurrentState.ExitState();
            CurrentState = States[nextStateKey];
            currentStateKey = nextStateKey;
            CurrentState.EnterState();
            _isTransitioningState = false;
        }

        private void FixedUpdate()
        {
            if (!_isTransitioningState)
                CurrentState.FixedUpdateState();
        }

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            CurrentState.OnTriggerEnter(collision);
        }

        protected void OnTriggerStay2D(Collider2D collision)
        {
            CurrentState.OnTriggerStay(collision);
        }

        protected void OnTriggerExit2D(Collider2D collision)
        {
            CurrentState.OnTriggerExit(collision);
        }
    }
}