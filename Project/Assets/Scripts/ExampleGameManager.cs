
using UnityEngine;
using m4k;
using m4k.Characters;
using m4k.Characters.Customization;
using m4k.Progression;
using m4k.BuildSystem;
using m4k.Items;
using m4k.Items.Crafting;
using m4k.SaveLoad;
using m4k.Incremental;

// TODO: GameTime contained serialization & data structure
// TODO: Editor listable ISavedObjects on SaveLoadData
[System.Serializable]
public class GameSaveData : GameDataBase {
    public float time;
    public int day;
    public RecordData recordData;
    public BuildingSystemData buildingSystemData;
    public InventoryData inventoryData;
    public CharacterData characterData;
    public CharacterCustomizationData customizationData;
    public ProgressionData progressionData;
    public CraftData craftData;
    public SpawnedObjectSavedData objectInstanceData;
    public IncrementalSaveData incrementalSaveData;

    public override void Serialize() {
        base.Serialize();
        this.time = GameTime.I.timeOfDay;
        this.day = GameTime.I.day;

        RecordManager.I?.Serialize(ref recordData);
        InventoryManager.I?.Serialize(ref inventoryData);
        ProgressionManager.I?.Serialize(ref progressionData);
        CharacterManager.I?.Serialize(ref characterData);

        BuildingSystem.I?.Serialize(ref buildingSystemData);
        CharacterCustomize.I?.Serialize(ref customizationData);
        CraftManager.I?.Serialize(ref craftData);
        SpawnManager.I?.Serialize(ref objectInstanceData);
        IncrementalManager.I?.Serialize(ref incrementalSaveData);
    }

    public override void Deserialize() {
        base.Deserialize();
        GameTime.I.timeOfDay = this.time;
        GameTime.I.day = this.day;

        RecordManager.I?.Deserialize(recordData);
        InventoryManager.I?.Deserialize(inventoryData);
        ProgressionManager.I?.Deserialize(progressionData);
        CharacterManager.I?.Deserialize(characterData);

        CharacterCustomize.I?.Deserialize(customizationData);
        BuildingSystem.I?.Deserialize(buildingSystemData);
        CraftManager.I?.Deserialize(craftData);
        SpawnManager.I?.Deserialize(objectInstanceData);
        IncrementalManager.I?.Deserialize(incrementalSaveData);
    }
}

// Execution order after manager classes that will be serialized. This allows singleton manager classes to initialize. Goal is to call SaveLoadManager.Init "between" the end of Awake and start of Start(for general initialization)
[DefaultExecutionOrder(100)]
public class ExampleGameManager : MonoBehaviour
{
    public KeyCode quickSaveKey = KeyCode.F5;
    public KeyCode quickLoadKey = KeyCode.F6;
    public KeyCode loadAutoKey = KeyCode.F8;
    SaveLoadData<GameSaveData> gameSaveData;
    int saveVersion = 1;

    void Awake() {
        gameSaveData = new SaveLoadData<GameSaveData>(new GameSaveData(), saveVersion);
        SaveLoadManager.Init(gameSaveData);
    }

    void Start() {
        if(IncrementalManager.I)
            IncrementalManager.I.onAutoSave += () => { 
                SaveLoadManager.saveLoadable.Save(SaveLoadManager.AutoSaveId); 
            };
    }

    private void Update() {
        if(Input.GetKeyDown(quickSaveKey)) {
            Debug.Log("Quicksave");
            SaveLoadManager.saveLoadable.QuickSave();
        }
        if(Input.GetKeyDown(quickLoadKey)) {
            Debug.Log("Quickload");
            SaveLoadManager.saveLoadable.QuickLoad();
        }
        if(Input.GetKeyDown(loadAutoKey)) {
            Debug.Log("Load autosave");
            SaveLoadManager.saveLoadable.Load(SaveLoadManager.AutoSaveId);
        }
    }
}
