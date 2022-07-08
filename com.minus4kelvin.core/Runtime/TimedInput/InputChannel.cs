
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace m4k.TimedInput {
/// <summary>
/// State manager for note queue. Each channel represents one input signature
/// </summary>
[Serializable]
public class InputChannel {
    public string id;
    public InputActionReference inputRef;
    
    // Possibly move notes to Channel:Note kvp queue in Composition or some other way too offload state/dynamic data from Channel
#if SERIALIZE_REFS
    [SerializeReference, SubclassSelector]
#endif
    public List<ITimedNote> notes = new List<ITimedNote>();

    public ITimedNote currentNote { get { 
        return notes.Count > 0 ? notes[0] : null; 
    }}

    public float startTime { get; private set; }
    public float cancelTime { get; private set; }
    public float performTime { get; private set; }

    public InputAction.CallbackContext currInputStart { get; private set; }
    public InputAction.CallbackContext currInputCancel { get; private set; }
    public InputAction.CallbackContext currInputPerform { get; private set; }

    public InputAction.CallbackContext lastInputStart { get; private set; }
    public InputAction.CallbackContext lastInputCancel { get; private set; }
    public InputAction.CallbackContext lastInputPerform { get; private set; }

    protected TimedInputManager manager;


    public virtual void Initialize(TimedInputManager manager) {
        this.manager = manager;
        inputRef.action.Enable();
        inputRef.action.performed += OnInputPerformed;
        inputRef.action.started += OnInputStarted;
        inputRef.action.canceled += OnInputCanceled;
        startTime = 0f;
        cancelTime = 0f;
        performTime = 0f;
    }

    public virtual void Cleanup() {
        inputRef.action.Disable();
        inputRef.action.performed -= OnInputPerformed;
        inputRef.action.started -= OnInputStarted;
        inputRef.action.canceled -= OnInputCanceled;
    }

    public virtual void OnUpdate() {
        if(notes.Count < 1) 
            return;

        if(notes[0].OnUpdate()) {
            NextNote();
        }
    }

    public virtual void HitNote(float timeDistance) {
        // Debug.Log($"{id}: Channel Hit {timeDistance}");
        manager.HitNote(notes[0], timeDistance);
    }

    public virtual void MissNote() {
        // Debug.Log($"{id}: Channel Miss");
        manager.MissNote(notes[0]);
    }

    /// <summary>
    /// Manually despawn note display instead of allowing note to be automatically despawned offscreen or with other implementation
    /// </summary>
    public virtual void DespawnNoteDisplay() {
        manager.DespawnNoteDisplay(notes[0]);
    }

    protected virtual void NextNote() {
        notes.RemoveAt(0);
    }


    protected virtual void OnInputStarted(InputAction.CallbackContext context) {
        lastInputStart = currInputStart;
        currInputStart = context;
        startTime = Time.time;
    }

    protected virtual void OnInputCanceled(InputAction.CallbackContext context) {
        lastInputCancel = currInputCancel;
        currInputCancel = context;
        cancelTime = Time.time;
    }

    protected virtual void OnInputPerformed(InputAction.CallbackContext context) {
        lastInputPerform = currInputPerform;
        currInputPerform = context;
        performTime = Time.time;
    }
}
}