# Simple Save Load System

Aims to minimize cross package dependencies while allowing serialization and deserialization of basic data structures. Currently requires main save data class to have explicit data structures and call Serialize and Deserialize methods through means such as singletons.

### Dependencies
- (optional)TextMeshPro
- Tested on Unity 2020.3+

### Todo
- Example, prefabs, tests

### Usage example
```c#
// Class that is serialized to file by serializer
public class ExampleGameSaveData : GameDataBase {
  ExampleSavedClass.ExampleSavedClassData exampleSavedClassData;

  public override void Serialize() {
    ExampleSavedClass.I.Serialize(ref exampleSavedClassData);
  }
  public override void Deserialize() {
    ExampleSavedClass.I.Deserialize(exampleSavedClassData);
  }
}

public class ExampleMainClass {
  SaveLoadData<ExampleGameSaveData> gameData;
  // Deserialize is called during Init so goal is to have accessors such as singletons already initialized before calling Init
  // Call in Awake if ExampleMainClass execution order after saved classes
  // Call in Start if ExampleMainClass execution order before saved classes
  void InitializeSaveData() {
    gameData = new SaveLoadData<ExampleGameSaveData>(new ExampleGameSaveData());
    SaveLoadManager.I.Init(gameData);
  }
}

// typically some kind of manager class
public class ExampleSavedClass : Singleton<ExampleSavedClass> {
  [System.Serializable]
  public class ExampleSavedClassData {
    int integer1 = 0;
  }
  int integer1 = 0;

  public void Serialize(ref ExampleSavedClassData data) {
    if(data == null) data = new ExampleSavedClassData();
    data.integer1 = integer1;
  }
  public void Deserialize(ExampleSavedClassData data) {
    this.integer1 = data.integer1;
  }
}
```

### SaveLoadManager
- Call Init with SaveLoadData<T> before use
- Holds ISaveLoadable ref for manually calling Save, Load, etc.

### SaveLoadData<T>
- T where T : GameDataBase or derivatives
- Instantiate with GameDataBase or derivative
- Pass instance to SaveLoadManager with Init method

### GameDataBase
- Base serialization class
- Overridable Serialize and Deserialize methods

### SaveDataSlot, SaveDataSlotHandler
- Simple UI for shared save/load slots, uses TMPro