
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace m4k.TimedInput {
public interface ITimedInputDisplay {
    void Initialize();
    void OnUpdate(float time);
    void SpawnNoteOnChannel(InputChannel channel, ITimedNote note);
    void HitNote(ITimedNote note, HitGradeScore gradeScore);
    void MissedNote(ITimedNote note);
    void DespawnNote(ITimedNote note);
    void UpdateScoreAndCombo(float score, int combo, float comboMult);
}

/// <summary>
/// Example implementation for displaying of timed input notes. 
/// </summary>
public class TimedInputDisplay : MonoBehaviour, ITimedInputDisplay {
    [System.Serializable]
    public struct ChannelRefs {
        public RectTransform lane;
        public RectTransform spawnPosition;
        public RectTransform actuationLine;
        public RectTransform button;
        public Image statusImage;
        public UnityEvent OnHit, OnMiss;
    }

    public struct SpawnedNote {
        public ITimedNote note;
        public RectTransform rectT;
        public ChannelRefs channelRefs;
        public Vector3 goalPoint;
    }

    public Transform notePrefab;
    public SerializableDictionary<string, ChannelRefs> channelIdUiDict;
    public TMP_Text timeText;
    public TMP_Text scoreText;
    public TMP_Text comboText;

    [Tooltip("Higher mult, further down notes scroll beyond actuation line. Useful to adjust if want notes to disappear sooner, or extremely long notes disappear before scrolling off screen")]
    [Range(2f, 10f)]
    public float noteOffscreenScrollMult = 2f;

    protected Dictionary<ulong, SpawnedNote> spawnedNotes = new Dictionary<ulong, SpawnedNote>();
    
    protected Queue<GameObject> notesPool = new Queue<GameObject>();
    protected Queue<ITimedNote> notesToDespawn = new Queue<ITimedNote>();

    bool updateTime, updateScore, updateCombo;


    public virtual void Initialize() {
        for(int i = 0; i < 10; ++i) {
            var go = Instantiate(notePrefab.gameObject);
            go.SetActive(false);
            notesPool.Enqueue(go);
        }
        updateTime = timeText;
        updateScore = scoreText;
        updateCombo = comboText;
    }

    public virtual void OnUpdate(float time) {
        if(updateTime)
            timeText.text = System.TimeSpan.FromSeconds(time).ToString("g");

        foreach(var entry in spawnedNotes) {
            var note = entry.Value.note;
            // queue despawn note when off canvas
            if(entry.Value.rectT.position.y + entry.Value.rectT.rect.height < 0f) {
                notesToDespawn.Enqueue(note);
                continue;
            }
            float noteLife = note.startTime - time;
            var noteProgress = 1 - (noteLife / note.timeToNoteStart);

            var newPos = Vector3.Lerp(entry.Value.channelRefs.spawnPosition.position, entry.Value.goalPoint, noteProgress / noteOffscreenScrollMult);

            entry.Value.rectT.position = newPos;
        }

        while(notesToDespawn.Count > 0)
            DespawnNote(notesToDespawn.Dequeue());
    }

    public virtual void SpawnNoteOnChannel(InputChannel channel, ITimedNote note) {
        if(!channelIdUiDict.TryGetValue(channel.id, out var channelRefs)) {
            Debug.LogWarning($"{channel.id} channel not found");
            return;
        }
        var go = PopOrSpawnNote();
        var rectT = go.transform as RectTransform;
        // height mult for note based on canvas height and time to reach actuation from top of lane
        var noteSizeMult = (channelRefs.spawnPosition.position.y - channelRefs.actuationLine.position.y) / note.timeToNoteStart;

        var noteHeight = (note.endTime - note.startTime) * noteSizeMult;

        // extrapolated goal pos for smooth scrolling past bottom of lane
        var goalPoint = channelRefs.actuationLine.position
            - (channelRefs.spawnPosition.position - channelRefs.actuationLine.position)
            * (noteOffscreenScrollMult - 1f);

        if(go.TryGetComponent<Image>(out var img)) {
            img.color = Color.gray;
        }
        rectT.SetParent(channelRefs.lane, false);
        go.SetActive(true);

        rectT.sizeDelta = new Vector2(channelRefs.button.sizeDelta.x, noteHeight);
        
        rectT.localPosition = new Vector3(0f, channelRefs.lane.position.y + noteHeight, 0f);

        var spawnedNote = new SpawnedNote() {
            note = note,
            rectT = rectT,
            channelRefs = channelRefs,
            goalPoint = goalPoint,
        };

        spawnedNotes.Add(note.id, spawnedNote);
    }

    public virtual void HitNote(ITimedNote note, HitGradeScore gradeScore) {
        if(spawnedNotes.TryGetValue(note.id, out var spawnedNote)) {
            spawnedNote.channelRefs.OnHit?.Invoke();
            spawnedNote.channelRefs.statusImage.color = Color.green;
            if(spawnedNote.rectT.TryGetComponent<Image>(out var img))
                img.color = Color.green;
            // display animated gradeScore score/name on lane
        }
    }

    public virtual void MissedNote(ITimedNote note) {
        if(spawnedNotes.TryGetValue(note.id, out var spawnedNote)) {
            spawnedNote.channelRefs.OnMiss?.Invoke();
            spawnedNote.channelRefs.statusImage.color = Color.red;
            if(spawnedNote.rectT.TryGetComponent<Image>(out var img))
                img.color = Color.red;
        }
    }

    public virtual void DespawnNote(ITimedNote note) {
        if(!spawnedNotes.TryGetValue(note.id, out var spawnedNote)) {
            Debug.LogWarning($"Tried to despawn nonexisting note");
            return;
        }
        FreeNote(spawnedNote.rectT.gameObject);
        spawnedNotes.Remove(note.id);
    }

    public virtual void UpdateScoreAndCombo(float score, int combo, float comboMult) {
        if(updateScore)
            scoreText.text = $"{score.ToString("G")}";
        if(updateCombo)
            comboText.text = $"{combo.ToString()} x{comboMult}";
    }

    GameObject PopOrSpawnNote() {
        if(notesPool.Count > 0) {
            return notesPool.Dequeue();
        }
        return Instantiate(notePrefab.gameObject);
    }

    void FreeNote(GameObject go) {
        go.SetActive(false);
        notesPool.Enqueue(go);
    }
}
}