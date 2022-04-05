using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Random IStates from list; interval between states; states should be recyclable
/// </summary>
public class RandomLoop : IState, IStateHandler {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentState { get; private set; }

    List<IState> _states;
    float _intervalBetweenStates;
    
    float _transitionTimer;

    public RandomLoop(float intervalBetweenStates, int priority = -1) {
        this._intervalBetweenStates = intervalBetweenStates;
        this.priority = priority;
        _states = new List<IState>();
    }

    public void AddState(IState state) {
        _states.Add(state);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        if(_states == null || _states.Count < 1) {
            Debug.LogError("Error: invalid list");
            return;
        }
        currentState = _states[0];
        currentState.OnEnter(processor);
        _transitionTimer = _intervalBetweenStates;
    }

    public bool OnUpdate() {
        if(currentState != null && currentState.OnUpdate()) {
            _transitionTimer = _intervalBetweenStates;
            currentState.OnExit();
            currentState = null;
        }

        if(_transitionTimer > 0f) {
            _transitionTimer -= Time.deltaTime;
        }
        else if(currentState == null) {
            int index = Random.Range(0, _states.Count);

            currentState = _states[index];
            currentState.OnEnter(processor);
        }

        return false;
    }

    public void OnExit() {
        currentState?.OnExit();
        _transitionTimer = 0f;
        currentState = null;
    }
}

[System.Serializable]
public class RandomLoopWrapper : StateWrapper {
    public float intervalBetweenStates;

    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<StateWrapper> states;

    public override IState GetState() {
        var state = new RandomLoop(intervalBetweenStates, priority);
        foreach(var s in states) {
            state.AddState(s.GetState());
        }
        return state;
    }
}

[CreateAssetMenu(fileName = "RandomLoopState", menuName = "Data/AI/States/RandomLoopState", order = 0)]
public class RandomLoopState : StateWrapperBase {
    public float intervalBetweenStates;

    [InspectInline(canCreateSubasset = true)]
    public List<StateWrapperBase> states;

    public override IState GetState() {
        var state = new RandomLoop(intervalBetweenStates, priority);
        foreach(var s in states) {
            state.AddState(s.GetState());
        }
        return state;
    }
}
}