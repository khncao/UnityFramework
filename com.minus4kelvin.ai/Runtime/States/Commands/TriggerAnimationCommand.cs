using UnityEngine;

namespace m4k.AI {
// animationclip length instead? combine anim+audio+particle?
// playable instead to reduce animator state ambiguity
public struct TriggerAnimation : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    string _paramName;
    int _stateLayer;

    public TriggerAnimation(int stateLayer, string paramName, int priority = -1, StateProcessor processor = null) {
        this._paramName = paramName;
        this._stateLayer = stateLayer;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        processor.anim?.SetTrigger(_paramName);
    }

    public bool OnUpdate() {
        if(processor.CheckAnimStateChangedToDefault(_stateLayer))
            return true;
        return false;
    }

    public void OnExit() {
        processor?.anim?.ResetTrigger(_paramName);
    }
}

[System.Serializable]
public class TriggerAnimationWrapper : StateWrapper {
    public int stateLayer;
    public string paramName;

    public override IState GetState() {
        return new TriggerAnimation(stateLayer, paramName, priority);
    }
}

[CreateAssetMenu(fileName = "TriggerAnimationCommand", menuName = "Data/AI/States/TriggerAnimationCommand", order = 0)]
public class TriggerAnimationCommand : StateWrapperBase {
    public int stateLayer;
    public string paramName;

    public override IState GetState() {
        return new TriggerAnimation(stateLayer, paramName, priority);
    }
}
}