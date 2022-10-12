using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
public class Follow : IState, ITargetHandler {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }
    public Transform target { get; set; }

    Path _pathCommand;
    float _squaredFollowThreshold;
    float _speedMult;

    public Follow(Transform target, float squaredFollowThreshold, float speedMult = 1f, int priority = -1, StateProcessor processor = null) {
        this.processor = processor;
        this.priority = priority;
        this.target = target;
        this._squaredFollowThreshold = squaredFollowThreshold;
        this._speedMult = speedMult;
        _pathCommand = new Path(this.target, speedMult);
        currentCommand = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        if(target == null) {
            Debug.LogWarning("No target in follow state");
        }
        _pathCommand.target = target;
        _pathCommand.OnEnter(processor);
        currentCommand = _pathCommand;
    }

    public bool OnUpdate() {
        bool pastThreshold = PastThreshold();
        if(currentCommand != null && 
            (_pathCommand.OnUpdate() || pastThreshold))
        {
            _pathCommand.OnExit();
            currentCommand = null;
        }
        if(currentCommand == null && pastThreshold) {
            _pathCommand.OnEnter(processor);
            currentCommand = _pathCommand;
        }
        return false;
    }

    public void OnExit() {
        _pathCommand.OnExit();
    }

    bool PastThreshold() {
        return (int)((processor.transform.position - target.position).sqrMagnitude) > _squaredFollowThreshold;
    }
}

[System.Serializable]
public class FollowWrapper : StateWrapper {
    public float squaredFollowThreshold;

    public override IState GetState() {
        return new Follow(null, squaredFollowThreshold, priority);
    }
}

[CreateAssetMenu(fileName = "FollowState", menuName = "Data/AI/States/FollowState", order = 0)]
public class FollowState : StateWrapperBase {
    public float squaredFollowThreshold;
    public float speedMult = 1f;

    public override IState GetState() {
        return new Follow(null, squaredFollowThreshold, speedMult, priority);
    }
}
}