using System.Collections.Generic;
using UnityEngine;

// TODO: line of sight timer lose target
namespace m4k.AI {
/// <summary>
/// Focus on target, facing target. Sub state machine with extra probability, distance, angle, cooldown conditions
/// </summary>
public class FocusTarget : IState, IStateHandler, ITargetHandler {

    // TODO: FocusTarget state profile or StatesProfile with better working conditions
    [System.Serializable]
    public struct FocusTargetState {
        public string description;
        public Conditions conditions;
        [Range(0, 1f)]
        public float probability;
        public Vector2 sqrDistanceRange;
        [Header("Leave angleWidth 0f to ignore angle")]
        [Range(0f, 360f)]
        public float angleWidth;
        [Range(0f, 360f)]
        public float angleForward;
        public float cooldown;
        
        [InspectInline]
        public StateWrapperBase stateWrapper;
    }

    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentState { get; private set; }
    public Transform target { get; set; }

    StateMachine _stateMachine;
    float _maxSqrDistance;
    FocusTargetState _currentFocusTargetState;
    List<FocusTargetState> _focusTargetStates;

    float _sqrDistance, _angle;
    Vector3 _dir;

    Dictionary<StateProcessor, Dictionary<StateWrapperBase, IState>> _processorStateCache;
    Dictionary<FocusTargetState, float> _lastStateTimes;

    public FocusTarget(Transform target, float maxSqrDistance, List<FocusTargetState> focusTargetStates, int priority = -1, StateProcessor processor = null) {
        this.processor = processor;
        this.priority = priority;
        this.target = target;
        this._maxSqrDistance = maxSqrDistance;
        this._focusTargetStates = focusTargetStates;

        this._processorStateCache = new Dictionary<StateProcessor, Dictionary<StateWrapperBase, IState>>();
        this._lastStateTimes = new Dictionary<FocusTargetState, float>();
        this.currentState = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _stateMachine = new StateMachine(processor);

        if(!target) {
            Debug.LogWarning("No target in FocusTarget state");
        }

        processor.movable.SetFaceTarget(target);
        Debug.Log($"{processor.gameObject} entered FocusTarget against {target.gameObject}");
    }

    public bool OnUpdate() {
        if(target == null) {
            return true;
        }
        _dir = target.position - processor.transform.position;
        _sqrDistance = _dir.sqrMagnitude;

        var flatDir = _dir;
        // flatDir -= processor.transform.up * Vector3.Dot(processor.transform.up, _dir);
        flatDir.y = 0f;
        _angle = Vector3.Angle(processor.transform.forward, flatDir);
        
        if(_sqrDistance > _maxSqrDistance) {
            return true;
        }
        if(_stateMachine.currentState == null || CurrentFocusTargetStateIsStale()) {
            GetState(out IState state, out FocusTargetState focusTargetState);
            if(state is ITargetHandler targetHandler) {
                targetHandler.target = this.target;
            }
            _stateMachine.ChangeState(state);
            // in case a state modified
            processor.movable.SetFaceTarget(target);
            
            if(_stateMachine.currentState == state) {
                if(_lastStateTimes.TryGetValue(focusTargetState, out var lastTime)) {
                    _lastStateTimes[focusTargetState] = Time.time;
                }
                else {
                    _lastStateTimes.Add(focusTargetState, Time.time);
                };
                _currentFocusTargetState = focusTargetState;
            }
        }
        _stateMachine.OnUpdate();

        return false;
    }

    public void OnExit() {
        _stateMachine.ChangeState(null);
        processor.movable.SetFaceTarget(null);
    }

    void GetState(out IState state, out FocusTargetState focusTargetState) {
        state = null;
        focusTargetState = _currentFocusTargetState;

        for(int i = 0; i < _focusTargetStates.Count; ++i) {
            if(_lastStateTimes.TryGetValue(_focusTargetStates[i], out var lastTime)) {
                if(Time.time - lastTime < _focusTargetStates[i].cooldown)
                    continue;
            }
            if(Random.value > _focusTargetStates[i].probability) {
                continue;
            }
            if(!_focusTargetStates[i].conditions.CheckCompleteReqs()) {
                continue;
            }
            if(_sqrDistance < _focusTargetStates[i].sqrDistanceRange.x ||
                _sqrDistance > _focusTargetStates[i].sqrDistanceRange.y) {
                continue;
            }
            if(_focusTargetStates[i].angleWidth != 0f) {
                Vector3 forward = Quaternion.AngleAxis(_focusTargetStates[i].angleForward, processor.transform.up) * processor.transform.forward;
                if(Vector3.Angle(forward, _dir) > _focusTargetStates[i].angleWidth) 
                    continue;
            }
            focusTargetState = _focusTargetStates[i];
            state = GetCachedOrNewState(_focusTargetStates[i].stateWrapper);
            return; // get first found
        }
    }

    IState GetCachedOrNewState(StateWrapperBase stateWrapper) {
        if(stateWrapper == null) {
            Debug.LogWarning("No stateWrapper");
            return null;
        }

        if(!_processorStateCache.TryGetValue(processor, out var cache)) {
            cache = new Dictionary<StateWrapperBase, IState>();
            _processorStateCache.Add(processor, cache);
        }
        if(!cache.TryGetValue(stateWrapper, out IState state)) {
            state = stateWrapper.GetState();
            cache.Add(stateWrapper, state);
        }
            
        return state;
    }

    bool CurrentFocusTargetStateIsStale() {
        if(_sqrDistance < _currentFocusTargetState.sqrDistanceRange.x ||
            _sqrDistance > _currentFocusTargetState.sqrDistanceRange.y) {
            return true;
        }
        if(_currentFocusTargetState.angleWidth != 0f) {
            Vector3 forward = Quaternion.AngleAxis(_currentFocusTargetState.angleForward, processor.transform.up) * processor.transform.forward;
            if(Vector3.Angle(forward, _dir) > _currentFocusTargetState.angleWidth) 
                return true;
        }
        return false;
    }
}

[System.Serializable]
public class FocusTargetWrapper : StateWrapper {
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag, then try processor currentTarget")]
    public Transform target;
    public string targetTag;
    [Header("Disengage sqrDistance")]
    public float maxSqrDistance;

    public List<FocusTarget.FocusTargetState> focusTargetStates;

    public override IState GetState() {
        if(!string.IsNullOrEmpty(targetTag)) {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
        return new FocusTarget(target, maxSqrDistance, focusTargetStates, priority);
    }
}

[CreateAssetMenu(fileName = "FocusTargetState", menuName = "Data/AI/States/FocusTargetState", order = 0)]
public class FocusTargetState : StateWrapperBase {
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag, then try processor currentTarget")]
    public Transform target;
    public string targetTag;
    [Header("Disengage sqrDistance")]
    public float maxSqrDistance;

    public List<FocusTarget.FocusTargetState> focusTargetStates;

    public override IState GetState() {
        if(!string.IsNullOrEmpty(targetTag)) {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
        return new FocusTarget(target, maxSqrDistance, focusTargetStates, priority);
    }
}
}