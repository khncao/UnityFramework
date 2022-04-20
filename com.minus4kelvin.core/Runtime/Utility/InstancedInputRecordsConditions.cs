using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace m4k {
/// <summary>
/// Contained conditions for temporal sequences such as ingame tutorials, realtime events, and objectives
/// </summary>
public class InstancedInputRecordsConditions : MonoBehaviour
{
    [System.Serializable]
    public class InputDetail {
        public string description;
        public InputActionReference inputRef;
        public string spriteId;

        public InputAction input { get { return inputRef.action; }}
    }
    public string description;
    
    [Header("Skip Input")]
    public InputAction skipInput;
    public List<InputDetail> inputDetails;
    public string inputSpriteAssetName = "controlSprites";

    [Header("Temporal Records")]
    public SerializableDictionary<string, long> records;
    public Conditions conditions;
    public InstancedInputRecordsConditions nextInstanceConditions;

    Dictionary<string, long> _recordsCount = new Dictionary<string, long>();
    bool _inputsComplete, _recordsComplete, _condsComplete;

    // Start is called before the first frame update
    void Start()
    {
        foreach(var r in records) 
            _recordsCount.Add(r.Key, 0);
        foreach(var i in inputDetails)
            i.input.Enable();
        RecordManager.I.onChangeRecord -= OnRecordChange;
        RecordManager.I.onChangeRecord += OnRecordChange;
        conditions.onChange -= OnConditionChange;
        conditions.onChange += OnConditionChange;
        conditions.RegisterChangeListener();
        OnConditionChange(conditions);

        skipInput.performed -= Skip;
        skipInput.performed += Skip;
        skipInput.Enable();
    }

    private void OnDestroy() {
        if(RecordManager.I)
            RecordManager.I.onChangeRecord -= OnRecordChange;
        conditions.onChange -= OnConditionChange;
        conditions.UnregisterChangeListener();
        skipInput.performed -= Skip;
        skipInput.Disable();

        foreach(var i in inputDetails)
            i.input.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if(inputDetails.Count > 0) {
            for(int i = 0; i < inputDetails.Count; ++i) {
                if(inputDetails[i] == null) 
                    continue;
                if(inputDetails[i].input.triggered) {
                    inputDetails.RemoveAt(i);
                    OnChange();
                }
            }
            if(inputDetails.Count == 0) {
                _inputsComplete = true;
                OnChange();
            }
        }
    }


    void OnRecordChange(Record rec) {
        if(_recordsCount.ContainsKey(rec.id)) {
            _recordsCount[rec.id] += rec.LastChange;

            CheckRecordsComplete();
            OnChange();
        }
    }

    void CheckRecordsComplete() {
        foreach(var r in _recordsCount) {
            if(r.Value < records[r.Key]) {
                return;
            }
        }

        _recordsComplete = true;
    }

    void OnConditionChange(Conditions conds) {
        _condsComplete = conditions.CheckCompleteReqs();
        OnChange();
    }

    void OnChange() {
        Feedback.I.DisplayNotification(ToString());

        if(_inputsComplete && _condsComplete && _recordsComplete) {
            if(nextInstanceConditions) {
                Feedback.I?.DisableNotification();
                nextInstanceConditions.gameObject.SetActive(true);
            }
            else
                Feedback.I.DisplayNotification($"{description} complete", 1f);
            Destroy(gameObject);
        }
    }

    public override string ToString() {
        string s = description + '\n';

        if(inputDetails.Count > 0)
            s += "Inputs:\n";
        for(int i = 0; i < inputDetails.Count; ++i) {
            if(!string.IsNullOrEmpty(inputDetails[i].spriteId)) {
                s += $"<sprite={inputSpriteAssetName} index={inputDetails[i].spriteId}>";
            }
            // else
            //     s += $"-{inputDetails[i].input.bindings[0].ToDisplayString()}";

            s += $"{inputDetails[i].description}\n";
        }

        if(records.Count > 0 && !_recordsComplete)
            s += "Records:\n";
        foreach(var r in records) {
            if(_recordsCount[r.Key] < r.Value)
                s += $"-{r.Key}: {_recordsCount[r.Key]}/{r.Value}\n";
        }

        if(conditions.conditions.Count > 0)
            s += "Conditions:\n";
        foreach(var c in conditions.conditions) {
            if(!c.CheckConditionMet())
                s += $"-{c.ToString()}\n";
        }
        return s;
    }

    void Skip(InputAction.CallbackContext context) {
        Destroy(gameObject);
    }
}
}