using UnityEngine;

namespace m4k.AI {
public struct Wait : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    float _waitTime;

    float _startTime;

    public Wait(float waitTime, int priority = -1, StateProcessor processor = null) {
        this._waitTime = waitTime;
        this.processor = processor;
        this.priority = priority;
        _startTime = 0f;
    }

    public void OnEnter(StateProcessor processor) {
        _startTime = Time.time;
    }

    public bool OnUpdate() {
        return (Time.time - _startTime) > _waitTime;
    }

    public void OnExit() {}
}

[System.Serializable]
public class WaitWrapper : StateWrapper {
    public float waitTime;

    public override IState GetState() {
        return new Wait(waitTime, priority);
    }
}

[CreateAssetMenu(fileName = "WaitCommand", menuName = "Data/AI/States/WaitCommand", order = 0)]
public class WaitCommand : StateWrapperBase {
    public float waitTime;

    public override IState GetState() {
        return new Wait(waitTime, priority);
    }
}
}