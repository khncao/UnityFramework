using UnityEngine;

namespace m4k.AI {
public struct Interact : IState, ITargetHandler {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public Transform target { get; set; }

    IStateInteractable _interactable;

    public Interact(IStateInteractable interactable, int priority = -1, StateProcessor processor = null) {
        this._interactable = interactable;
        this.processor = processor;
        this.priority = priority;
        this.target = null;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        if(_interactable == null && target) {
            if(target.TryGetComponent<IStateInteractable>(out IStateInteractable interactable)) 
                _interactable = interactable;
        }
        if(_interactable == null) {
            Debug.LogWarning("No interactable");
        }
        _interactable.OnStateInteract(this);
    }

    public bool OnUpdate() => true;

    public void OnExit() { }
}

[System.Serializable]
public class InteractWrapper : StateWrapper {
    [Header("Target->WrappingState(detector)")]
    [Tooltip("GameObject with component that implements IStateInteractable")]
    public GameObject target;

    public override IState GetState() {
        IStateInteractable t = null;
        target.TryGetComponent<IStateInteractable>(out t);
        return new Interact(t, priority);
    }
}

[CreateAssetMenu(fileName = "InteractCommand", menuName = "Data/AI/States/InteractCommand", order = 0)]
public class InteractCommand : StateWrapperBase {
    [Header("Target->WrappingState(detector)")]
    [Tooltip("GameObject with component that implements IStateInteractable")]
    public GameObject target;

    public override IState GetState() {
        IStateInteractable t = null;
        target.TryGetComponent<IStateInteractable>(out t);
        return new Interact(t, priority);
    }
}
}