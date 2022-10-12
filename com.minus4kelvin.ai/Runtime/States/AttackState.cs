using UnityEngine;

namespace m4k.AI {
public class Attack : IState, ITargetHandler {
    [System.Serializable]
    public struct Data {
        public int stateLayer;
        public string triggerParam;
        [Range(0f, 1f)]
        public float attackNormalizedStartTime;
        [Range(0f, 1f)]
        public float attackNormalizedDuration;
        public ScriptableObject toolItem;
    }

    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public Transform target { get; set; }

    Data data;
    IToolInteract _tool;
    bool _attacking;

    public Attack(Data data, int priority) {
        this.data = data;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _attacking = false;
        _tool = processor.currentWieldTool;
        
        if(!string.IsNullOrEmpty(data.triggerParam))
            processor.anim?.SetTrigger(data.triggerParam);
    }

    public bool OnUpdate() {
        var stateInfo = processor.currAnimStateInfo[data.stateLayer];

        if(!_attacking
        && stateInfo.normalizedTime > data.attackNormalizedStartTime
        && (stateInfo.normalizedTime - data.attackNormalizedStartTime) < data.attackNormalizedDuration) {
            // Debug.Log("Enable attack");
            if(_tool != null) {
                _tool.StartInteract(null, target);
            }
            _attacking = true;
        }
        else if(_attacking
        && (stateInfo.normalizedTime - data.attackNormalizedStartTime) > data.attackNormalizedDuration) {
            // Debug.Log("Disable attack");
            if(_tool != null) {
                _tool.StopInteract();
            }
            _attacking = false;
        }
        if(processor.CheckAnimStateChangedToDefault(data.stateLayer))
            return true;
        return false;
    }

    public void OnExit() {
        _attacking = false;
        if(!string.IsNullOrEmpty(data.triggerParam))
            processor?.anim?.ResetTrigger(data.triggerParam);
    }
}

[CreateAssetMenu(fileName = "AttackState", menuName = "Data/AI/States/AttackState", order = 0)]
public class AttackState : StateWrapperBase {
    public Attack.Data data;

    public override IState GetState() {
        return new Attack(data, priority);
    }
}
}