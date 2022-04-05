using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Path to closest StateInteractableKeyEvent with matching key; interact
/// </summary>
public struct PathKeyInteract : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }

    string _key;
    StateInteractableKeyEvent _interactable;

    public PathKeyInteract(string key, int priority = -1, StateProcessor processor = null) {
        this._key = key;
        this._interactable = null;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _interactable = StateInteractableManager.I.GetClosestInteractableKey(_key, processor.transform);
        if(_interactable) {
            processor.movable.SetTarget(_interactable.transform);
            processor.ToggleProximityTrigger(true);
            // processor.movable.OnArrive += OnArrive;
            // processor.onArrive += OnArrive;
        }
        else {
            Debug.LogError("Path key target not found");
        }
    }

    public bool OnUpdate() {
        if(!processor.movable.IsMoving) {
            if(_interactable != null && _interactable.CanStateInteract(this))
                _interactable.OnStateInteract(this);
            return true;
        }
        return false;
    }

    public void OnExit() {
        // processor.movable.OnArrive -= OnArrive;
        // processor.onArrive -= OnArrive;
        processor.movable.Stop();
    }

    // void OnArrive() {

    // }
}

[System.Serializable]
public class PathKeyInteractWrapper : StateWrapper {
    public string key;

    public override IState GetState() {
        return new PathKeyInteract(key, priority);
    }
}

[CreateAssetMenu(fileName = "PathKeyInteractCommand", menuName = "Data/AI/States/PathKeyInteractCommand", order = 0)]
public class PathKeyInteractCommand : StateWrapperBase {
    public string key;

    public override IState GetState() {
        return new PathKeyInteract(key, priority);
    }
}
}