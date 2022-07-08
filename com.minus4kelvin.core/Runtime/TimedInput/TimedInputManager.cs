
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

namespace m4k.TimedInput {
[System.Serializable]
public class HitGradeScore {
    [Tooltip("eg. Perfect, Great, Okay, Bad")]
    public string name;
    public float maxTimeDistance;
    public float score;
}

/// <summary>
/// Simple implementation of timed input system manager. 
/// </summary>
public class TimedInputManager : Singleton<TimedInputManager> {

    public ITimedInputDisplay inputDisplay;
    public InputActionAsset inputAsset;

    [Tooltip("Time taken for first note pivot point to reach actuation line. Affects global speed of notes")]
    public float noteSpawnProjectionTime = 5f;
    public float zeroComboMult = 1f;
    [Tooltip("Time radius of a standard tap note")]
    public float tapNoteRadius = 0.25f;
    public float missScorePenalty = 100f;

    // public List<float> gradeScores;
    [Tooltip("Index as hit grading. 0 index as lowest, default score. Distance threshold ignored in element 0")]
    public List<HitGradeScore> gradeScores;

    [Tooltip("Key(combo count ascending), Value(multiplier exclusive)")]
    public SerializableDictionary<int, float> comboMultiplierThresholds;


    public UnityEvent<ITimedNote> OnNoteHit;
    public UnityEvent<ITimedNote> OnNoteMiss;

    [Header("Test")]
    public TimedNoteComposition testComposition;
    public bool spawnTapNotes;
    public bool spawnHoldNotes;

    public int MaxGrade { get { return gradeScores.Count - 1; }}

    List<InputChannel> inputChannels;

    TimedNoteComposition _currentComposition;

    float playTime;
    bool inPlay;
    int currentCombo;
    float currentScore;
    float currentComboMult;
    ulong noteCounter;

    public InputActionReference shiftMod;

    protected void Start() {
        inputDisplay = GetComponentInChildren<ITimedInputDisplay>();
        if(inputDisplay == null) {
            Debug.LogWarning("No input display interface found");
        }
        inputDisplay.Initialize();

        shiftMod.action.Enable();

        _currentComposition = null;
        currentComboMult = zeroComboMult;
        AssignComposition(testComposition);
        BeginPlay();
    }
    
    protected void OnDestroy() {
        shiftMod.action.Disable();
        EndComposition();
        EndPlay();
    }

    protected void Update() {
        if(!inPlay) 
            return;
        playTime += Time.deltaTime;

// test composition serialization for future runtime composition feature
// #if DEVELOPMENT_BUILD || UNITY_EDITOR
//         if(shiftMod.action.IsPressed() && Input.GetKeyDown(KeyCode.S)) {
//             string json = JsonUtility.ToJson(testComposition);
//             System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "Data", "test.json"), json);
//         }
// #endif

        for(int i = 0; i < inputChannels.Count; ++i) {

// test manual dynamic note spawning
#if DEVELOPMENT_BUILD || UNITY_EDITOR 
            if(inputChannels[i].inputRef.action.WasPressedThisFrame()) {
                if(shiftMod.action.IsPressed()) {
                    int randMax = 0;
                    if(spawnTapNotes) randMax++;
                    if(spawnHoldNotes) randMax++;
                    if(randMax == 0) continue;

                    int rand = UnityEngine.Random.Range(0, randMax);
                    ITimedNote note;
                    if(rand == 0) {
                        note = new TapTimedNote(GetId(), inputChannels[i], tapNoteRadius, noteSpawnProjectionTime);
                    }
                    else {
                        var randLength = UnityEngine.Random.Range(1f, 3f);
                        note = new HoldTimedNote(GetId(), inputChannels[i], randLength, 0.5f, noteSpawnProjectionTime);
                    }
                    RegisterDynamicTimedNote(inputChannels[i], note);
                }
            }
#endif

            inputChannels[i].OnUpdate();
        }

        inputDisplay.OnUpdate(playTime);
    }

    /// <summary>
    /// Register runtime generated note to input channel. Null note will enqueue default note at default time to cursor.
    /// </summary>
    public void RegisterDynamicTimedNote(string inputChannelId, ITimedNote note) {
        InputChannel channel = _currentComposition.inputChannels.Find(x=>x.id == inputChannelId);
        if(channel == null) {
            Debug.Log($"{inputChannelId} channel id not found");
            return;
        }
        RegisterDynamicTimedNote(channel, note);
    }

    public void RegisterDynamicTimedNote(InputChannel channel, ITimedNote note) {
        if(note == null) {
            Debug.Log("Null note");
            return;
        }
        if(note.startTime < Time.time) {
            Debug.LogWarning("Note start time already passed");
            return;
        }
        if(channel.notes.Count > 0 && 
        note.startTime < channel.notes[channel.notes.Count - 1].endTime) {
            Debug.Log("Overlap with previous note in channel");
            return;
        }
        // Debug.Log($"Register note: {channel.id}");
        channel.notes.Add(note);
        inputDisplay.SpawnNoteOnChannel(channel, note);
    }

    public void HitNote(ITimedNote note, float timeDistance) {
        var grade = GetHitGrade(timeDistance);
        if(grade == null) {
            Debug.LogWarning($"Grade for distance {timeDistance} not found");
            return;
        }
        Debug.Log($"{note.channel.id}: {grade.name}");
        inputDisplay.HitNote(note, grade);
        SetScoreAndCombo(currentScore + GetScore(grade), currentCombo + 1);
        OnNoteHit?.Invoke(note);
    }

    public void MissNote(ITimedNote note) {
        Debug.Log($"{note.channel.id}: Miss");
        inputDisplay.MissedNote(note);
        SetScoreAndCombo(currentScore - missScorePenalty, 0);
        OnNoteMiss?.Invoke(note);
    }

    public void DespawnNoteDisplay(ITimedNote note) {
        inputDisplay.DespawnNote(note);
    }

    /// <summary>
    /// Assign a specific, maybe precomposed, list of TimedNotes to be read
    /// </summary>
    public void AssignComposition(TimedNoteComposition composition) {
        if(_currentComposition != null) {
            Debug.Log("Composition already assigned");
            return;
        }
        _currentComposition = composition;
        inputChannels = _currentComposition.inputChannels;

        _currentComposition.Initialize(this);
    }

    public void EndComposition() {
        _currentComposition.Cleanup();
        _currentComposition = null;
    }

    public ulong GetId() {
        return noteCounter++;
    }

    public HitGradeScore GetHitGrade(float timeDistance) {
        if(gradeScores.Count < 1) {
            Debug.LogError("No grade scores");
            return null;
        }
        if(timeDistance < 0) 
            timeDistance = -timeDistance;

        for(int i = gradeScores.Count - 1; i > 0; --i) {
            if(timeDistance < gradeScores[i].maxTimeDistance) {
                return gradeScores[i];
            }
        }
        return gradeScores[0];
    }

    public HitGradeScore GetHitGrade(int grade) {
        if(gradeScores.Count < 1) {
            Debug.LogError("No grade scores");
            return null;
        }
        if(grade < 0 || grade >= gradeScores.Count) {
            Debug.LogWarning("Grade input out of range, clamping");
            grade = Mathf.Clamp(grade, 0, gradeScores.Count - 1);
        }
        return gradeScores[grade];
    }

    public float GetScore(HitGradeScore grade) {
        return grade.score * currentComboMult;
    }

    protected void BeginPlay() {
        inPlay = true;
        playTime = 0f;
        noteCounter = 0;
        SetScoreAndCombo(0f, 0);
    }

    protected void EndPlay() {
        inPlay = false;
    }

    protected void SetScoreAndCombo(float score, int combo) {
        if(combo == 0) {
            currentComboMult = zeroComboMult;
        }
        else {
            foreach(var entry in comboMultiplierThresholds) {
                if(combo < entry.Key) {
                    break;
                }
                currentComboMult = entry.Value;
            }
        }

        currentScore = score;
        currentCombo = combo;

        inputDisplay.UpdateScoreAndCombo(score, combo, currentComboMult);
    }
}
}