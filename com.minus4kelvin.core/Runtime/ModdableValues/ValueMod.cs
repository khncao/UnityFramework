using System;

namespace m4k.ModdableValues {
// TODO: possibly change to base class for extendability
/// <summary>
/// Modifier struct for float values. Contains parameters for a processing step in getting the final value of a ModdableValue.
/// </summary>
[Serializable]
public struct ValueMod {
    public object source;
    public float _addedValue;
    public float multiplier;
    
    public float AddedValue {
        get {
            return AddedModdable == null ? _addedValue : AddedModdable.Value;
        }
    }
    
    public ModdableValue AddedModdable { get; private set; }

    bool multiplyAfterAddedValue;
    
    public ValueMod(float value, object source = null, float multiplier = 1f, bool multiplyAfterAddedValue = true)
    {
        this._addedValue = value;
        this.multiplier = multiplier;
        this.source = source;
        this.multiplyAfterAddedValue = multiplyAfterAddedValue;
        AddedModdable = (source != null && source is ModdableValue m) 
            ? m : null;
    }

    public ValueMod(ModdableValue addedModdable, object source = null, float multiplier = 1f, bool multiplyAfterAddedValue = true) {
        this._addedValue = 0f;
        this.multiplier = multiplier;
        this.source = source;
        this.multiplyAfterAddedValue = multiplyAfterAddedValue;
        this.AddedModdable = addedModdable;
    }

    public float ProcessValue(float inputValue) {
        return multiplyAfterAddedValue 
        ? inputValue + (inputValue + AddedValue) * multiplier
        : inputValue + (inputValue * multiplier) + AddedValue;
    }
}
}