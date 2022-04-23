// using System.Collections;
// using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace m4k.SaveLoad {
public class SaveDataSlotHandler : MonoBehaviour
{
    public SaveDataSlot quickSaveSlot;
    public SaveDataSlot autoSaveSlot;
    public SaveDataSlot[] slots;
#if TMPRO
    public TMPro.TMP_Text slotsLabel;
#else
    public Text slotsLabel;
#endif
    public GameObject confirmOverwritePanel;
    
    Action<int> _onPressSlot;
    int _overwriteSlotId;

    void Start() {
        InitSlots();
        RefreshSlots();
    }

    void InitSlots() {
        if(quickSaveSlot) 
            ConfigSlot(quickSaveSlot, SaveLoadManager.QuickSaveId);
        if(autoSaveSlot) 
            ConfigSlot(autoSaveSlot, SaveLoadManager.AutoSaveId);

        for(int i = 0; i < slots.Length; i++) {
            ConfigSlot(slots[i], i);
        }
    }

    void ConfigSlot(SaveDataSlot slot, int id) {
        if(slot.TryGetComponent<Button>(out var button)) {
            button.onClick.AddListener(()=>OnButton(id));
            slot.slotId.text = id.ToString();
        }
    }

    public void RefreshSlots() 
    {
        if(quickSaveSlot) 
            RefreshSlot(quickSaveSlot, SaveLoadManager.QuickSaveId);
        if(autoSaveSlot) 
            RefreshSlot(autoSaveSlot, SaveLoadManager.AutoSaveId);

        for(int i = 0; i < slots.Length; i++) {
            RefreshSlot(slots[i], i);
        }
    }

    void RefreshSlot(SaveDataSlot slot, int id) {
        var fileName = $"{SaveLoadManager.SaveFilePrefix}{id}";

        // if(PlayerPrefs.HasKey(fileName)) 
        // {
        //     slot.sceneName.text = PlayerPrefs.GetString(fileName + "_scene");
        //     slot.playTime.text = FormatTime( PlayerPrefs.GetInt(fileName + "_time") );
        // }
        SaveLoadManager.SaveMetaData.TryGetData(fileName, "scene", out var sceneName);
        SaveLoadManager.SaveMetaData.TryGetData(fileName, "time", out var timeString);
        
        slot.sceneName.text = sceneName;
        slot.playTime.text = string.IsNullOrEmpty(timeString) ? "" : FormatTime(int.Parse(timeString));
    }

    public void OpenSaveSlots() {
        slotsLabel.text = "SAVE GAME";
        _onPressSlot = SaveLoadManager.saveLoadable.Save;
        gameObject.SetActive(true);
    }

    public void OpenLoadSlots() {
        slotsLabel.text = "LOAD GAME";
        _onPressSlot = SaveLoadManager.saveLoadable.Load;
        gameObject.SetActive(true);
    }

    public void ConfirmOverwrite() {
        _onPressSlot(_overwriteSlotId);
        confirmOverwritePanel.SetActive(false);
        // gameObject.SetActive(false);
        RefreshSlots();
    }
    bool isSaving;
    void OnButton(int id) {
        isSaving = slotsLabel.text == "SAVE GAME";
        if(isSaving) {
            if(slots[id].playTime.text != "") {
                confirmOverwritePanel.SetActive(true);
                _overwriteSlotId = id;
                return;
            }
        }

        _onPressSlot(id);
        // gameObject.SetActive(false);
        if(isSaving)
            RefreshSlots();
    }

    string FormatTime(int iTime)
    {			
        TimeSpan t = TimeSpan.FromSeconds( iTime );
        
        /// You can add more digits by adding more digits eg: {1:D2}:{2:D2}:{3:D2}:{4:D2} to also display milliseconds.
        return string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
    }
}
}