using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;
using m4k.Characters;

namespace m4k.AI {
/// <summary>
/// AI agent that contains and manages the primary state machine. Passed into IState classes for access to character related actions such as navigation, inventory, and animation. Derive from, or directly modify to add state actions.
/// </summary>
public class StateProcessor : MonoBehaviour, IStateHandler
{
    public static List<StateProcessor> instances { get; } = new List<StateProcessor>();
    public static System.Action<StateProcessor> onAddProcessor, onRemoveProcessor;

    public StatesProfile statesProfile;
    public ProcessorProximityTrigger proximityTrigger;
    public INavMovable movable;
    public InventoryComponent inventory;
    public Animator anim;
    public Collider col;
    [Tooltip("Visual transform parent for wield instance")]
    public Transform wieldParent;
    [Tooltip("Pivot from which wield physics checks pivot/originate")]
    public Transform wieldPivot;
    public ScriptableObject defaultWieldIItem;
    public bool conscriptable = true;
    public float detectionCooldown = 1f;
    public float viewAngles = 180f;
    public bool visualize = true;
    public bool applyState = true;
    public bool autoInit = false;

    public Action onStateComplete, onArrive;
    public Action onStateChange;

    public bool HasState { get { return currentState != null; } }
    public bool isStopped { get; set; }
    public IState currentState { get; private set; }

    public IItemPrefab currentWieldItem { get; private set; }
    public IToolInteract currentWieldTool { get; private set; }
    public GameObject currentWieldInstance { get; private set; }

    protected float lastAbortTime;
    protected bool abortingTask;

    protected StateMachine stateMachine;

    protected Queue<IState> eventStateQueue = new Queue<IState>();
    protected Dictionary<IItemPrefab, GameObject> itemInstances = new();

    public AnimatorStateInfo[] currAnimStateInfo, prevAnimStateInfo, defaultAnimStateInfo;

    bool initialized = false;

    public IState GetCurrentNestedState(int maxDepth = 5) {
        IState state = currentState;
        int depth = 0;
        while(state is IStateHandler stateHandler && depth < maxDepth) {
            state = stateHandler.currentState;
            depth++;
            if(depth == maxDepth) 
                Debug.LogWarning("MaxDepth reached");
        }
        return state;
    }

    protected virtual void Start() {
        if(autoInit) {
            Init();
        }
    }

    public void Init() {
        if(initialized) return;
        initialized = true;
        movable = GetComponentInChildren<INavMovable>();
        if(movable == null)
            movable = GetComponentInParent<INavMovable>();
        if(movable == null) {
            Debug.LogError($"No movable interface found on {gameObject}");
            return;
        }
        
        if(!inventory)
            inventory = GetComponentInChildren<InventoryComponent>();

        if(!anim) anim = GetComponent<Animator>();
        currAnimStateInfo = new AnimatorStateInfo[anim.layerCount];
        prevAnimStateInfo = new AnimatorStateInfo[anim.layerCount];
        defaultAnimStateInfo = new AnimatorStateInfo[anim.layerCount];
        for(int i = 0; i < anim.layerCount; ++i) {
            defaultAnimStateInfo[i] = anim.GetCurrentAnimatorStateInfo(i);
            currAnimStateInfo[i] = prevAnimStateInfo[i] = defaultAnimStateInfo[i];
        }

        if(defaultWieldIItem && defaultWieldIItem is IItemPrefab item) {
            WieldItem(item);
        }

        if(!col)
            col = GetComponentInChildren<Collider>();

        if(!proximityTrigger) {
            Debug.LogError($"{gameObject} needs taskInteractObj TaskInteractTrigger on navigation target object");
            return;
        }
        proximityTrigger.processor = this;
        proximityTrigger.processorCol = col;
        ToggleProximityTrigger(false);

        stateMachine = new StateMachine(this);
        stateMachine.OnStateComplete += OnStateComplete;

        TryChangeState(GetState(), true);
    }

    public virtual void OnEnable() {
        foreach(var s in statesProfile.persistentEventStateListeners) {
            if(!s.eventSO || !s.stateWrapperBase) 
                continue;

            s.eventSO.AddListener(()=>
                TryChangeState(s.stateWrapperBase.GetState(), s.forceChange, s.queueIfBusy));
        }

        if(!conscriptable) 
            return;
        instances.Add(this);
        onAddProcessor?.Invoke(this);
    }

    public virtual void OnDisable() {
        foreach(var s in statesProfile.persistentEventStateListeners) {
            if(!s.eventSO || !s.stateWrapperBase) 
                continue;
            s.eventSO.CleanupObj(this);
        }
        foreach(var item in itemInstances) {
            item.Value.SetActive(false);
        }

        if(!conscriptable) 
            return;
        instances.Remove(this);
        onRemoveProcessor?.Invoke(this);
    }

    public virtual void OnUpdate() {
        stateMachine.OnUpdate();
    }

    public virtual void Resume() {
        OnEnable();
        isStopped = false;
        TryChangeState(GetState(), true);
    }

    public virtual void Stop() {
        TryChangeState(null, true);
        isStopped = true;
        OnDisable();
    }

    public void QueueState(IState state) {
        eventStateQueue.Enqueue(state);
    }

    public IState GetState() {
        var state = statesProfile.GetState(this);
        if(eventStateQueue.Count > 0) {
            if(eventStateQueue.Peek().priority > state.priority)
                state = eventStateQueue.Dequeue();
        }
        return state;
    }

    public virtual bool TryChangeState(IState state, bool forceChange = false, bool addToQueue = false) {
        if(isStopped) {
            return false;
        }
        if(forceChange || stateMachine.CanChangeState(state)) {
            stateMachine.ChangeState(state);
            // Debug.Log(state);
            currentState = state;
            return true;
        }
        else if(addToQueue) {
            Debug.Log($"Added {state} to queue");
            QueueState(state);
            return false;
        }
        Debug.Log($"Failed to change state: {state}");
        return false;
    }

    protected virtual void OnStateComplete() {
        // Debug.Log($"{currentState} complete"); 
        CleanupState();
        onStateComplete?.Invoke();
        TryChangeState(GetState());
    }

    public virtual void AbortState() {
        lastAbortTime = Time.time;
        abortingTask = true;

        CleanupState();
    }

    protected virtual void CleanupState() {
        ToggleProximityTrigger(false);
        
        currentState = null;
    }

    public virtual void OnArrive() {
        onArrive?.Invoke();
        ToggleProximityTrigger(false);
        movable.Stop();
    }

    public virtual void ToggleProximityTrigger(bool b) {
        proximityTrigger?.gameObject.SetActive(b);
    }

    public virtual GameObject GetOrSpawnItemInstance(IItemPrefab item) {
        if(!itemInstances.TryGetValue(item, out GameObject instance)) {
            GameObject prefab = item.GetPrefab();
            if(!prefab) return null;
            instance = Instantiate(prefab);
            itemInstances.Add(item, instance);
        }
        return instance;
    }

    public virtual void FreeItemInstance(GameObject instance) {
        currentWieldItem = null;
        currentWieldInstance = null;
    }

    public virtual void WieldItem(IItemPrefab item) {
        GameObject instance = GetOrSpawnItemInstance(item);
        if(instance) {
            instance.SetActive(true);
            instance.transform.SetParent(wieldParent, false);
            if(instance.TryGetComponent<IToolInteract>(out var tool)) {
                tool.Init(visualize, applyState, gameObject, (ScriptableObject)item);
                tool.pivot = wieldPivot;
            }
            currentWieldTool = tool;
            currentWieldItem = item;
            currentWieldInstance = instance;
        }
    }

    public virtual void UpdateAnimStateInfo() {
        for(int i = 0; i < anim.layerCount; ++i) {
            prevAnimStateInfo[i] = currAnimStateInfo[i];
            currAnimStateInfo[i] = anim.GetCurrentAnimatorStateInfo(i);
        }
    }

    public virtual bool CheckAnimStateChangedToDefault(int layer) {
        UpdateAnimStateInfo();

        if(prevAnimStateInfo[layer].shortNameHash != currAnimStateInfo[layer].shortNameHash 
        && currAnimStateInfo[layer].shortNameHash == defaultAnimStateInfo[layer].shortNameHash
        )
            return true;

        return false;
    }

}
}
