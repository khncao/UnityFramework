using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Handled by TaskManager. Wrapper for StateSequenceState.
/// </summary>
[System.Serializable]
public class Task : IState
{
    public string description { get; private set; }
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }

    StateSequence _sequenceState;
    bool _completed;

    public Task(string description, int priority = -1, StateProcessor processor = null) {
        this.priority = priority;
        this.description = description;
        this.processor = processor;
        _sequenceState = new StateSequence(priority, processor);
    }

    public void Enqueue(IState command) {
        _sequenceState.Enqueue(command);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        _sequenceState.OnEnter(processor);
        _completed = false;
    }

    public bool OnUpdate() {
        if(_sequenceState.OnUpdate()) {
            TaskManager.I.CompleteTask(this);
            _completed = true;
            return true;
        }
        return false;
    }

    public void OnExit() {
        _sequenceState.OnExit();
        // if(processor.itemArranger.isActive) { // if has task items
        //     // create drop for player/ai pick up or return items to original storage location
        // }
        // Re register incomplete task
        // if(!_completed) {
        //     TaskManager.I.RegisterTask(this);
        //     Debug.Log("Reregistered task");
        // }
    }
}

[System.Serializable]
public class TaskWrapper : StateWrapper {
    public string description;
    
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public List<StateWrapper> states;
    

    public override IState GetState() {
        var state = new Task(description, priority);
        foreach(var s in states) {
            state.Enqueue(s.GetState());
        }
        return state;
    }
}

[CreateAssetMenu(fileName = "TaskState", menuName = "Data/AI/States/TaskState", order = 0)]
public class TaskState : StateWrapperBase {
    public string description;
    
    [InspectInline(canCreateSubasset = true)]
    public List<StateWrapperBase> states;

    public override IState GetState() {
        var state = new Task(description, priority);
        foreach(var s in states) {
            state.Enqueue(s.GetState());
        }
        return state;
    }
}
}