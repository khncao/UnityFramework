using System;

namespace m4k.ModdableValues {
// TODO: possibly change to generic base class for extendability
/// <summary>
/// Modifier struct for float values. Contains parameters for a processing step in getting the final value of a ModdableValue. Also serves as container/wrapper for using ModdableValues as modifiers
/// </summary>
[Serializable]
public struct ValueMod {
    public object source;
    public float multiplier;
    public int order;

    float _addedValue;
    
    public float AddedValue {
        get {
            return AddedModdable == null ? _addedValue : AddedModdable.Value;
        }
    }
    
    public ModdableValue AddedModdable { get; private set; }

    bool multiplyAfterAddedValue;
    
    public ValueMod(float value, 
                    object source = null, 
                    int order = 0, 
                    float multiplier = 1f, 
                    bool multiplyAfterAddedValue = true) {
        this._addedValue = value;
        this.multiplier = multiplier;
        this.source = source;
        this.order = order;
        this.multiplyAfterAddedValue = multiplyAfterAddedValue;
        AddedModdable = (source != null && source is ModdableValue m) 
            ? m : null;
    }

    public ValueMod(ModdableValue addedModdable, 
                    object source = null, 
                    int order = 0, 
                    float multiplier = 1f, 
                    bool multiplyAfterAddedValue = true) {
        this._addedValue = 0f;
        this.multiplier = multiplier;
        this.source = source;
        this.order = order;
        this.multiplyAfterAddedValue = multiplyAfterAddedValue;
        this.AddedModdable = addedModdable;
    }

    public float ProcessValue(float inputValue) {
        return multiplyAfterAddedValue 
        ? (inputValue + AddedValue) * multiplier
        : (inputValue * multiplier) + AddedValue;
    }
}
}