using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Listening reaction state that tries to assign task state to processor
/// </summary>
public class TaskListener : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    
    IState _state;
    bool _hasTask;

    public TaskListener(IState state, int priority = -1, StateProcessor processor = null) {
        this._state = state;
        this.priority = priority;
        this.processor = processor;
        _hasTask = false;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        if(TaskManager.I.TryGetTask(processor) != null) {
            _hasTask = true;
            return;
        }
        TaskManager.I.onTaskAdded -= TryGetTask;
        TaskManager.I.onTaskAdded += TryGetTask;
        _state.OnEnter(processor);
    }

    public bool OnUpdate() {
        return _hasTask || _state.OnUpdate();
    }

    public void OnExit() {
        _state.OnExit();
        TaskManager.I.onTaskAdded -= TryGetTask;
        _hasTask = false;
    }

    void TryGetTask() {
        TaskManager.I.TryGetTask(processor);
    }
}

[System.Serializable]
public class TaskListenerWrapper : StateWrapper {
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public StateWrapper state;

    public override IState GetState() {
        return new TaskListener(state.GetState(), priority);
    }
}

[CreateAssetMenu(fileName = "TaskListenerState", menuName = "Data/AI/States/TaskListenerState", order = 0)]
public class TaskListenerState : StateWrapperBase {
    [InspectInline(canCreateSubasset = true)]
    public StateWrapperBase state;

    public override IState GetState() {
        return new TaskListener(state.GetState(), priority);
    }
}
}