
namespace m4k.SaveLoad {
/// <summary>
/// Interface to access common methods in generic class. Used in SaveLoadData.
/// </summary>
public interface ISaveLoadable {
    void Save(int index);
    void Load(int index);
    void QuickSave();
    void QuickLoad();
    void Continue();
}
}