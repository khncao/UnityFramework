using System.IO;
using UnityEngine;

namespace m4k.SaveLoad {
[System.Serializable]
public class SaveMetaData {
    public int mostRecentSaveIndex = -1;
    public SerializableDictionary<string, SerializableDictionary<string, string>> data = new SerializableDictionary<string, SerializableDictionary<string, string>>();

    public void AddOrModifyData(string saveName, string entryName, string value) {
        Validate(saveName, entryName, value);

        if(!data.TryGetValue(saveName, out var entries)) {
            entries = new SerializableDictionary<string, string>();
            data.Add(saveName, entries);
        }
        if(entries.ContainsKey(entryName)) {
            entries[entryName] = value;
        }
        else {
            entries.Add(entryName, value);
        }
    }

    public bool TryGetData(string saveName, string entryName, out string value) {
        Validate(saveName, entryName);

        if(data.TryGetValue(saveName, out var entries)) {
            return entries.TryGetValue(entryName, out value);
        }
        value = "";
        return false;
    }

    void Validate(string saveName, string entryName, string value = "value") {
        if(string.IsNullOrEmpty(saveName)) 
            Debug.LogError("Invalid saveName");
        if(string.IsNullOrEmpty(entryName))
            Debug.LogError("Invalid entryName");
        if(string.IsNullOrEmpty(value))
            Debug.LogWarning("Value null or empty");
    }
}
/// <summary>
/// Handles basic game save and load operations with GameDataBase and derivatives
/// </summary>
/// <typeparam name="T"></typeparam>
public class SaveLoadData<T> : ISaveLoadable where T : GameDataBase 
{
    float refPlayStartTime;
    float loadedPlayTime;
    float totalPlayTime;
    string filePath;
    T dataInstance;
    int version;
    int latestLoadId;

    public SaveLoadData(T data, int version, string saveFilePrefix = "Save") {
        this.dataInstance = data;
        this.version = version;

        SaveLoadManager.SaveFilePrefix = saveFilePrefix;
        
        if(SaveLoadManager.SaveMetaData == null)
            SaveLoadManager.SaveMetaData = new SaveMetaData();
        LoadMetaData();
    }

    public void OnNewGame() {
        refPlayStartTime = Time.time;
        loadedPlayTime = 0f;
        totalPlayTime = 0f;
    }

    public void Continue() {
        // if(!PlayerPrefs.HasKey("latestSaveFileId")) {
        //     Debug.Log("continue pressed without recent save");
        //     return;
        // }
        // var saveIndex = PlayerPrefs.GetInt("latestSaveFileId", 0);
        // Load(saveIndex);
        if(SaveLoadManager.SaveMetaData.mostRecentSaveIndex != -1)
            Load(SaveLoadManager.SaveMetaData.mostRecentSaveIndex);
    }

    public void QuickSave() {
        Save(SaveLoadManager.QuickSaveId);
    }
    public void QuickLoad() {
        Load(SaveLoadManager.QuickSaveId);
    }

    public void AutoSaveSlot() {
        Save(SaveLoadManager.AutoSaveId);
    }
    public void AutoSaveLatestLoad() {
        Save(latestLoadId);
    }

    public void Save(int index) {
        var fileName = $"{SaveLoadManager.SaveFilePrefix}{index}";
        totalPlayTime = loadedPlayTime + Time.time - refPlayStartTime;
        UpdateMetaData(fileName, index);

        SaveJson(dataInstance, fileName + ".json");
    }

    public void Load(int index) {
        if(!SceneHandler.I.isMainMenu) {
            SaveLoadManager.loadIndex = index;
            SceneHandler.I.ReturnToMainMenu();
        }
        var fileName = $"{SaveLoadManager.SaveFilePrefix}{index}";

        LoadJson(fileName + ".json");
        refPlayStartTime = (int)Time.time;
        latestLoadId = index;
    }

    void UpdateMetaData(string fileName, int index) {
        // PlayerPrefs.SetInt("latestSaveFileId", index); // for continue
        
        // PlayerPrefs.SetInt(fileName, index);
        // PlayerPrefs.SetString(fileName + "_scene", SceneHandler.I.activeScene.name);
        // PlayerPrefs.SetInt(fileName + "_time", (int)totalPlayTime);

        SaveLoadManager.SaveMetaData.mostRecentSaveIndex = index;

        SaveLoadManager.SaveMetaData.AddOrModifyData(fileName, "scene", SceneHandler.I.activeScene.name);
        SaveLoadManager.SaveMetaData.AddOrModifyData(fileName, "time", ((int)totalPlayTime).ToString());

        SaveMetaData();
    }

    void SaveJson(T data, string fileName) {
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        data.version = 1;
        data.playTime = totalPlayTime;
        data.sceneName = SceneHandler.I.activeScene.name;

        data.Serialize();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    T LoadJson(string fileName) {
        filePath = Path.Combine(Application.persistentDataPath, fileName);
        string json;

        if(File.Exists(filePath)) 
            json = File.ReadAllText(filePath);
        else {
            Debug.LogError("Load file does not exist");
            return null;
        }
        var data = JsonUtility.FromJson<T>(json);

        data.Deserialize();

        loadedPlayTime = data.playTime;
        SceneHandler.I.LoadSceneByName(data.sceneName, true, true);
        return data;
    }

    public string SaveMetaData() {
        filePath = Path.Combine(Application.persistentDataPath, "SaveMetaData.json");

        string json = JsonUtility.ToJson(SaveLoadManager.SaveMetaData, true);
        File.WriteAllText(filePath, json);
        return json;
    }

    public void LoadMetaData() {
        filePath = Path.Combine(Application.persistentDataPath, "SaveMetaData.json");
        string json;

        if(File.Exists(filePath)) 
            json = File.ReadAllText(filePath);
        else {
            json = SaveMetaData();
        }
        SaveLoadManager.SaveMetaData = JsonUtility.FromJson<SaveMetaData>(json);
    }
}
}