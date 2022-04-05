using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Singleton access to common save/load functions and saveLoadable. Save data slots and metadata are stored in PlayerPrefs.
/// </summary>
namespace m4k.SaveLoad {
public class SaveLoadManager {
    // Static survive reload of persistent scene and flag index of scene to load
    public static int loadIndex = -1;
    public static ISaveLoadable saveLoadable;

    public Action onSaveDone, onLoadDone, onQuickSave, onQuickLoad;

    public static void Init(ISaveLoadable newSaveLoadable) {
        saveLoadable = newSaveLoadable;
        TryLoadScene();
    }

    public static void TryLoadScene() {
        if(loadIndex >= 0) {
            saveLoadable.Load(loadIndex);
            loadIndex = -1;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Reset() {
        loadIndex = -1;
        saveLoadable = null;
    }
}
}