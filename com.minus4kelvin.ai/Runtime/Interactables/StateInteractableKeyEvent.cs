using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k.AI {
public class StateInteractableKeyEvent : MonoBehaviour, IStateInteractable
{
    public List<string> keys;
    public string processorAnimTrigger;
    public UnityEvent onStateInteract;

    private void Start() {
        OnEnable();
    }
    
    private void OnEnable() {
        if(!StateInteractableManager.I) return;
        foreach(var k in keys)
            StateInteractableManager.I.RegisterInteractableKey(k, this);
        StateInteractableManager.I.stateInteractables.RegisterInstance(this);
    }
    private void OnDisable() {
        if(!StateInteractableManager.I) return;
        foreach(var k in keys)
            StateInteractableManager.I.UnregisterInteractableKey(k, this);
        StateInteractableManager.I.stateInteractables.UnregisterInstance(this);
    }

    public void OnStateInteract(IState state) {
        onStateInteract.Invoke();
        if(!string.IsNullOrEmpty(processorAnimTrigger)) {
            state.processor.anim.SetTrigger(processorAnimTrigger);
        }
    }

    public bool CanStateInteract(IState state) {
        return true;
    }
}
}