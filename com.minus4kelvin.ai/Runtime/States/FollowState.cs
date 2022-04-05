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

    public Follow(Transform target, float squaredFollowThreshold, int priority = -1, StateProcessor processor = null) {
        this.processor = processor;
        this.priority = priority;
        this.target = target;
        this._squaredFollowThreshold = squaredFollowThreshold;
            _pathCommand = new Path(this.target);
        currentCommand = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _pathCommand.OnEnter(processor);
        currentCommand = _pathCommand;

        if(target == null) {
            Debug.LogWarning("No target in follow state");
        }
        _pathCommand.target = target;
    }

    public bool OnUpdate() {
        if(currentCommand != null && _pathCommand.OnUpdate()) {
            _pathCommand.OnExit();
            currentCommand = null;
        }
        else if(currentCommand == null && PastThreshold()) {
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
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag")]
    public Transform target;
    public string targetTag;
    public float squaredFollowThreshold;

    public override IState GetState() {
        if(!string.IsNullOrEmpty(targetTag)) {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
        return new Follow(target, squaredFollowThreshold, priority);
    }
}

[CreateAssetMenu(fileName = "FollowState", menuName = "Data/AI/States/FollowState", order = 0)]
public class FollowState : StateWrapperBase {
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag")]
    public Transform target;
    public string targetTag;
    public float squaredFollowThreshold;

    public override IState GetState() {
        if(!string.IsNullOrEmpty(targetTag)) {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
        return new Follow(target, squaredFollowThreshold, priority);
    }
}
}