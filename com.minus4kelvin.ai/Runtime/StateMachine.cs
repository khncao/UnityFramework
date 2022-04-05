// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

namespace m4k.AI {
public class StateMachine
{
    public event System.Action OnStateComplete;
    public StateProcessor processor { get; private set; }
    public IState currentState { get; private set; }

    public StateMachine(StateProcessor processor) {
        this.processor = processor;
    }

    public virtual void OnUpdate() {
        if(currentState == null)
            return;

        if(currentState.OnUpdate()) {
            currentState.OnExit();
            currentState = null;
            OnStateComplete?.Invoke();
        }
    }

    public void ChangeState(IState newState) {
        currentState?.OnExit();
        currentState = newState;
        newState?.OnEnter(processor);
    }

    /// <summary>
    /// True if no current state or current state priority lower than parameter state priorty
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public bool CanChangeState(IState state) {
        return currentState == null || currentState.priority < state.priority;
    }
}
}