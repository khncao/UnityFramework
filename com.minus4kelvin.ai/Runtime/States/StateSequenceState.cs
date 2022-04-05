using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Super state; ordered sequence of command states to perform complex task.
/// </summary>
[System.Serializable]
public class StateSequence : IState, IStateHandler
{
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentState { get; private set; }
    
    LinkedList<IState> _commandList;
    LinkedListNode<IState> _currentNode;
    

    public StateSequence(int priority = -1, StateProcessor processor = null) {
        this.priority = priority;
        this.processor = processor;
        this.currentState = null;
        _commandList = new LinkedList<IState>();
        _currentNode = null;
    }

    public void Enqueue(IState command) {
        _commandList.AddLast(command);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        _currentNode = _commandList.First;
        currentState = _currentNode.Value;
        currentState.OnEnter(processor);
    }

    public bool OnUpdate() {
        if(currentState != null && currentState.OnUpdate()) {
            currentState.OnExit();

            if(_currentNode == _commandList.Last) {
                return true;
            }
            else {
                _currentNode = _currentNode.Next;
                currentState = _currentNode.Value;
                currentState.OnEnter(processor);
            }
        }
        return false;
    }

    public void OnExit() {
        currentState?.OnExit();
        currentState = null;
    }
}

[System.Serializable]
public class StateSequenceWrapper : StateWrapper {
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<StateWrapper> states;

    public override IState GetState() {
        var state = new StateSequence(priority);
        foreach(var s in states) {
            state.Enqueue(s.GetState());
        }
        return state;
    }
}

[CreateAssetMenu(fileName = "StateSequenceState", menuName = "Data/AI/States/StateSequenceState", order = 0)]
public class StateSequenceState : StateWrapperBase {
    [InspectInline(canCreateSubasset = true)]
    public List<StateWrapperBase> states;

    public override IState GetState() {
        var state = new StateSequence(priority);
        foreach(var s in states) {
            state.Enqueue(s.GetState());
        }
        return state;
    }
}
}