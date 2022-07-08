
using System;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.InputSystem.Interactions;

namespace m4k.TimedInput {
/// <summary>
/// State timed note object contract
/// </summary>
public interface ITimedNote {
    ulong id { get; set; }
    float startTime { get; }
    float endTime { get; }
    /// <summary>
    /// startTime - timeToNoteStart = spawnTime
    /// </summary>
    float timeToNoteStart { get; }
    InputChannel channel { get; }

    /// <summary>
    /// Poll TimedInputManager->InputChannel->CurrentNote(this)
    /// </summary>
    /// <returns>True to trigger note completion and InputChannel move to next note</returns>
    bool OnUpdate();
}

[Serializable]
public struct TapTimedNote : ITimedNote {
    public ulong id { get; set; }
    public InputChannel channel { get; private set; }
    public float startTime { get; private set; }
    public float endTime { get; private set; }
    public float timeToNoteStart { get; private set; }

    public float timePivot { get { return _noteTimePivot; }}
    public float targetTimeRadius { get { return _targetTimeRadius; }}

    [SerializeField]
    float _noteTimePivot;
    [SerializeField]
    float _targetTimeRadius;

    public TapTimedNote(ulong id, InputChannel channel, float targetTimeRadius, float timeToNoteStart) {
        this.id = id;
        this.channel = channel;
        this._targetTimeRadius = targetTimeRadius;
        this.timeToNoteStart = timeToNoteStart;

        this._noteTimePivot = Time.time + timeToNoteStart;
        this.startTime = _noteTimePivot - targetTimeRadius;
        this.endTime = _noteTimePivot + targetTimeRadius;
    }
    
    public bool OnUpdate() {
        if(Time.time > endTime) {
            channel.MissNote();
            return true;
        }
        if(Time.time < startTime) {
            return false;
        }
        if(channel.inputRef.action.WasPerformedThisFrame()) {
            float absTimeFromTarget = timePivot - channel.startTime;

            absTimeFromTarget = absTimeFromTarget < 0 ? -absTimeFromTarget : absTimeFromTarget;

            channel.HitNote(absTimeFromTarget);
            channel.DespawnNoteDisplay();
            return true;
        }
        return false;
    }
}

/// <summary>
/// One of many ways to implement held notes
/// </summary>
[Serializable]
public struct HoldTimedNote : ITimedNote {
    public ulong id { get; set; }
    public InputChannel channel { get; private set; }
    public float startTime { get { return _startTime; }}
    public float endTime { get; private set; }
    public float timeToNoteStart { get; private set; }

    [SerializeField]
    float _startTime;
    [SerializeField]
    float _timeLength;
    [SerializeField]
    float _tickTime;
    // [Tooltip("Time depth of scoring pivot at start and end of hold note")]
    // [SerializeField]
    // float _pivotDepth;
    // int _tickGrade;

    float _tickTimer; // TODO: move tick management to channel/manager

    public HoldTimedNote(ulong id, InputChannel channel, float timeLength, float tickTime, float timeToNoteStart) {
        this.id = id;
        this.channel = channel;
        this._timeLength = timeLength;
        this._tickTime = tickTime;
        this.timeToNoteStart = timeToNoteStart;

        this._startTime = Time.time + timeToNoteStart;
        this.endTime = _startTime + timeLength;
        this._tickTimer = 0f;
    }

    public bool OnUpdate() {
        if(Time.time < startTime) {
            return false;
        }
        if(Time.time > endTime) {
            return true;
        }
        // TODO: move tick management to channel/manager
        _tickTimer += Time.deltaTime;
        if(_tickTimer > _tickTime) {
            _tickTimer = 0f;

            OnTick();
        }
        return false;
    }

    public void OnTick() {
        if(channel.startTime <= channel.cancelTime) {
            channel.MissNote();
        }
        else {
            channel.HitNote(0f);
        }
    }
}

// wip
// [Serializable]
// public class DragTimedNote : ITimedNote {
//     public ulong id { get; set; }
//     public InputChannel channel { get; private set; }
//     public float startTime { get; private set; }
//     public float endTime { get; private set; }
//     public float timeToNoteStart { get; private set; }

//     [SerializeField]
//     float _pointRadius;
//     [SerializeField]
//     float _timeToNextPoint;
//     [SerializeField]
//     Vector3[] _points; // position of button inputs

//     public DragTimedNote(ulong id, InputChannel channel, float pointRadius, float timeToNextPoint, Vector3[] points, float timeToNoteStart) {
//         this.id = id;
//         this.channel = channel;
//         this.timeToNoteStart = timeToNoteStart;
//         this._pointRadius = pointRadius;
//         this._timeToNextPoint = timeToNextPoint;
//         this._points = points;

//         this.startTime = Time.time + timeToNoteStart;
//     }

//     public bool OnUpdate() {
//         return true;
//     }
// }
}