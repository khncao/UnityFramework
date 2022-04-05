using System.IO;
using UnityEngine;

namespace m4k.SaveLoad {
/// <summary>
/// Handles basic game save and load operations with GameDataBase and derivatives
/// </summary>
/// <typeparam name="T"></typeparam>
public class SaveLoadData<T> : ISaveLoadable where T : GameDataBase 
{
    float refPlayStartTime;
    float totalPlayTime;
    string filePath, saveName;
    T dataInstance;

    public SaveLoadData(T data) {
        this.dataInstance = data;
    }
    public void NewGame() {
        refPlayStartTime = Time.time;
        totalPlayTime = 0f;
    }
    public void Continue()
    {
        if(!PlayerPrefs.HasKey("latestSaveFileId")) 
            Debug.Log("continue pressed without recent save");

        var saveIndex = PlayerPrefs.GetInt("latestSaveFileId", 0);
        Load(saveIndex);
    }
    public void QuickSave() {
        Save(100);
        Feedback.I.SendLine("Quicksaved", true);
    }
    public void QuickLoad() {
        Load(100);
    }

    public void Save(int index) {
        var fileName = string.Format("saveFile{0}", index);
        totalPlayTime += Time.time - refPlayStartTime;
        SetPlayerPrefs(fileName, index);

        SaveJson(dataInstance, fileName + ".json", index);
    }
    public void Load(int index) {
        if(!SceneHandler.I.isMainMenu) {
            SaveLoadManager.loadIndex = index;
            SceneHandler.I.ReturnToMainMenu();
        }
        var fileName = string.Format("saveFile{0}", index);

        LoadJson(fileName + ".json", index);
        refPlayStartTime = (int)Time.time;
    }

    void SetPlayerPrefs(string fileName, int index) {
        PlayerPrefs.SetInt("latestSaveFileId", index); // for continue
        PlayerPrefs.SetInt(fileName, index);

        PlayerPrefs.SetString(fileName + "_scene", SceneHandler.I.activeScene.name);
        // PlayerPrefs.SetInt(fileName + "_money", InventoryManager.I.mainInventory.Currency);
        PlayerPrefs.SetInt(fileName + "_time", (int)totalPlayTime);
    }

    void SaveJson(T data, string fileName, int index) {
        filePath = Path.Combine(Application.persistentDataPath, fileName);

        data.version = 1;
        data.playTime = totalPlayTime;
        data.sceneName = SceneHandler.I.activeScene.name;

        data.Serialize();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    T LoadJson(string fileName, int index) {
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

        totalPlayTime = data.playTime;
        SceneHandler.I.LoadSceneByName(data.sceneName, true, true);
        return data;
    }
}
}