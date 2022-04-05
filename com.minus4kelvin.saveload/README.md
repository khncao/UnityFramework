# Simple Save Load System

### Dependencies
- https://github.com/khncao/com.minus4kelvin.core 
  - Singleton, Feedback and SceneHandler
- (optional)TextMeshPro
- Tested on Unity 2020.3.6f1+

### Todo
- Example, prefabs, tests
- Simple registry based implementation for arbitrary save-loaded monobehaviours

### Usage
```c#
public class GameSaveData : GameDataBase {}

public class Example {
  SaveLoadData<GameSaveData> gameData;

  public void Start() {
    gameData = new SaveLoadData<GameSaveData>(new GameSaveData());
    SaveLoadManager.I.Init(gameData);
  }
}

public class Example2 {
  public void Save(int index) {
    SaveLoadManager.I.saveLoadable.Save(index);
  }
  public void Load(int index) {
    SaveLoadManager.I.saveLoadable.Load(index);
  }
}
```

### SaveLoadManager
- Singleton
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