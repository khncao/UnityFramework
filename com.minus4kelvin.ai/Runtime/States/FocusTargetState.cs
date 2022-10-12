using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Focus on target, facing target. Sub state machine with extra probability, distance, angle, cooldown conditions
/// </summary>
public class FocusTarget : IState, IStateHandler, ITargetHandler {
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

    [System.Serializable]
    public struct Data {
        public float maxSqrDistance;
        public List<FocusTargetState> focusTargetStates;
    }

    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentState { get; private set; }
    public Transform target { get; set; }

    Data data;

    StateMachine _stateMachine;
    FocusTargetState _currentFocusTargetState;
    float _sqrDistance, _angle;
    Vector3 _dir;

    Dictionary<StateWrapperBase, IState> _stateCache;
    Dictionary<FocusTargetState, float> _lastStateTimes;

    public FocusTarget(Data data, int priority) {
        this.data = data;
        this.priority = priority;

        _stateCache = new Dictionary<StateWrapperBase, IState>();
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
    }

    public bool OnUpdate() {
        if(target == null) {
            return true;
        }
        _dir = target.position - processor.transform.position;
        _sqrDistance = _dir.sqrMagnitude;
        if(_sqrDistance > data.maxSqrDistance) {
            return true;
        }
        var flatDir = _dir;
        // flatDir -= processor.transform.up * Vector3.Dot(processor.transform.up, _dir);
        flatDir.y = 0f;
        _angle = Vector3.Angle(processor.transform.forward, flatDir);
        
        if(_stateMachine.currentState == null 
        || CurrentFocusTargetStateIsStale()) {
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

        for(int i = 0; i < data.focusTargetStates.Count; ++i) {
            if(_lastStateTimes.TryGetValue(data.focusTargetStates[i], out var lastTime)) {
                if(Time.time - lastTime < data.focusTargetStates[i].cooldown)
                    continue;
            }
            if(Random.value > data.focusTargetStates[i].probability) {
                continue;
            }
            if(!data.focusTargetStates[i].conditions.CheckCompleteReqs()) {
                continue;
            }
            if(_sqrDistance < data.focusTargetStates[i].sqrDistanceRange.x ||
                _sqrDistance > data.focusTargetStates[i].sqrDistanceRange.y) {
                continue;
            }
            if(data.focusTargetStates[i].angleWidth != 0f) {
                Vector3 forward = Quaternion.AngleAxis(data.focusTargetStates[i].angleForward, processor.transform.up) * processor.transform.forward;
                if(Vector3.Angle(forward, _dir) > data.focusTargetStates[i].angleWidth) 
                    continue;
            }
            focusTargetState = data.focusTargetStates[i];
            state = GetCachedOrNewState(data.focusTargetStates[i].stateWrapper);
            return; // get first found
        }
    }

    IState GetCachedOrNewState(StateWrapperBase stateWrapper) {
        if(stateWrapper == null) {
            Debug.LogWarning("No stateWrapper");
            return null;
        }
        if(!_stateCache.TryGetValue(stateWrapper, out IState state)) {
            state = stateWrapper.GetState();
            _stateCache.Add(stateWrapper, state);
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

[CreateAssetMenu(fileName = "FocusTargetState", menuName = "Data/AI/States/FocusTargetState", order = 0)]
public class FocusTargetState : StateWrapperBase {
    public FocusTarget.Data data;

    public override IState GetState() {
        return new FocusTarget(data, priority);
    }
}
}