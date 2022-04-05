
namespace m4k.SaveLoad {
/// <summary>
/// Derive with additional fields and objects to save/load. Use SaveLoadData intialized with derivative to handle save/load.
/// </summary>
[System.Serializable]
public abstract class GameDataBase {
    public int version;
    public string sceneName;
    public float playTime;

    public virtual void Serialize() {

    }
    public virtual void Deserialize() {
        
    }
}
}