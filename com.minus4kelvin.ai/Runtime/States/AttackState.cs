using UnityEngine;

namespace m4k.AI {
public struct Attack : IState, ITargetHandler {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public Transform target { get; set; }

    string _weaponId;
    string _stateName;
    string _triggerParam;
    int _stateLayer;
    float _normalizedTransitionTime;
    float _attackNormalizedStartTime;
    float _attackNormalizedDuration;

    IToolInteract _toolInteract;
    bool _attacking;

    public Attack(string weaponId, float attackNormalizedStartTime, float attackNormalizedDuration, string stateName, float normalizedTransitionTime, int stateLayer, string triggerParam, int priority = -1, StateProcessor processor = null) {
        this._triggerParam = triggerParam;
        this._stateLayer = stateLayer;
        this.processor = processor;
        this.target = null;
        this.priority = priority;
        this._stateName = stateName;
        this._normalizedTransitionTime = normalizedTransitionTime;
        this._weaponId = weaponId;
        this._attackNormalizedStartTime = attackNormalizedStartTime;
        this._attackNormalizedDuration = attackNormalizedDuration;
        this._attacking = false;
        this._toolInteract = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _attacking = false;
        _toolInteract = processor.GetComponent<IToolInteract>();
        
        if(!string.IsNullOrEmpty(_stateName))
            processor.anim?.CrossFade(_stateName, _normalizedTransitionTime, _stateLayer);
        else if(!string.IsNullOrEmpty(_triggerParam))
            processor.anim?.SetTrigger(_triggerParam);
    }

    public bool OnUpdate() {
        var stateInfo = processor.currAnimStateInfo[_stateLayer];

        if(!_attacking && stateInfo.IsName(_stateName)
        && stateInfo.normalizedTime > _attackNormalizedStartTime
        && (stateInfo.normalizedTime - _attackNormalizedStartTime) < _attackNormalizedDuration) {
            // Debug.Log("Enable attack");
            if(_toolInteract != null) {
                _toolInteract.StartInteract(_weaponId, target);
            }
            _attacking = true;
        }
        else if(_attacking
        && (stateInfo.normalizedTime - _attackNormalizedStartTime) > _attackNormalizedDuration) {
            // Debug.Log("Disable attack");
            if(_toolInteract != null) {
                _toolInteract.StopInteract();
            }
            _attacking = false;
        }
        if(processor.CheckAnimStateChangedToDefault(_stateLayer))
            return true;
        return false;
    }

    public void OnExit() {
        _attacking = false;
        if(!string.IsNullOrEmpty(_triggerParam))
            processor?.anim?.ResetTrigger(_triggerParam);
    }
}

[System.Serializable]
public class AttackStateWrapper : StateWrapper {
    [Header("StateName->triggerParam")]
    public int stateLayer;
    public string stateName;
    [Tooltip("Crossfade time when using stateName. Not used by triggerParam")]
    [Range(0f, 1f)]
    public float normalizedTransitionTime;
    public string triggerParam;
    [Range(0f, 1f)]
    public float attackNormalizedStartTime;
    [Range(0f, 1f)]
    public float attackNormalizedDuration;

    [Header("ID for using specific weapon tool")]
    public string weaponId;

    public override IState GetState() {
        return new Attack(weaponId, attackNormalizedStartTime, attackNormalizedDuration, stateName, normalizedTransitionTime, stateLayer, triggerParam, priority);
    }
}

[CreateAssetMenu(fileName = "AttackState", menuName = "Data/AI/States/AttackState", order = 0)]
public class AttackState : StateWrapperBase {
    [Header("StateName->triggerParam")]
    public int stateLayer;
    public string stateName;
    [Tooltip("Crossfade time when using stateName. Not used by triggerParam")]
    [Range(0f, 1f)]
    public float normalizedTransitionTime;
    public string triggerParam;
    [Range(0f, 1f)]
    public float attackNormalizedStartTime;
    [Range(0f, 1f)]
    public float attackNormalizedDuration;

    [Header("ID for using specific weapon tool")]
    public string weaponId;

    public override IState GetState() {
        return new Attack(weaponId, attackNormalizedStartTime, attackNormalizedDuration, stateName, normalizedTransitionTime, stateLayer, triggerParam, priority);
    }
}
}