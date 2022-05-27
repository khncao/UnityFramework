// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace m4k.ModdableValues {
/// <summary>
/// Identifiable base value that returns a final Value after processing registered ValueMod modifiers. ModdableValues can be registered as dependencies that automatically trigger this ModdableValue to recalculate on change. 
/// </summary>
[Serializable]
public class ModdableValue {
    public string id;
    public float baseValue;

    public event Action<float, ModdableValue> onChange;

    [NonSerialized]
    List<ValueMod> modifiers = new List<ValueMod>();

    // List of moddable values dependent on this value. ex: Damage is dependent on Strength. Can be used to update dependent values when this value changes; also to remove this value from dependent values when this object is manually destroyed by calling OnDestroy
    [NonSerialized]
    List<ModdableValue> dependantModdables = new List<ModdableValue>();

	bool isDirty = true;
	float _value;

    public virtual float Value {
        get {
            if(isDirty) {
                CalculateFinalValue();
            }
            return _value;
        }
    }

    public ModdableValue(float baseValue, string id = "") {
        this.baseValue = baseValue;
        this.id = id;
        isDirty = true;
    }

    public void SetBaseValue(float baseValue) {
        isDirty = true;
        this.baseValue = baseValue;
        onChange?.Invoke(Value, this);
    }

 
    public virtual void AddModifier(ValueMod mod) {
        isDirty = true;
        modifiers.Add(mod);
        modifiers.Sort((x, y) => x.order.CompareTo(y.order));

        onChange?.Invoke(Value, this);
    }

    public virtual bool RemoveModifier(ValueMod mod) {
        if (modifiers.Remove(mod)) {
            isDirty = true;
            onChange?.Invoke(Value, this);
            return true;
        }
        return false;
    }


    public virtual void AddModdableModifier(ModdableValue moddable, 
                                            object source = null, 
                                            int order = 0, 
                                            float multiplier = 1f, 
                                            bool multiplyAfterAddedValue = true) 
    {
        if(moddable.ContainsModdableModifier(this)) {
            Debug.LogError($"{moddable.ToString()} depends on this moddable {ToString()}; circle dependency");
            return;
        }
        if(moddable.dependantModdables.FindIndex(x=>x==this) != -1) {
            Debug.LogError($"{ToString()} already dependent on {moddable.ToString()}");
            return;
        }
        moddable.onChange += OnDependeeModdableChange;
        moddable.dependantModdables.Add(this);
        if(source == null) 
            source = moddable;
        var mod = new ValueMod(moddable, source, order, multiplier, multiplyAfterAddedValue);
        AddModifier(mod);
    }

    public virtual void RemoveModdableModifier(ModdableValue moddable) {
        if(modifiers.RemoveAll(x => 
        (x.AddedModdable != null 
        && x.AddedModdable == moddable)) > 0) {
            moddable.onChange -= OnDependeeModdableChange;
            isDirty = true;
            onChange?.Invoke(Value, this);
            moddable.dependantModdables.Remove(this);
        }
    }

    public virtual bool RemoveAllModifiersFromSource(object source) {
        var foundMods = modifiers.FindAll(x=>x.source == source);
        if(foundMods.Count < 1) 
            return false;

        for (int i = foundMods.Count - 1; i >= 0; --i) {
            if(foundMods[i].source is ModdableValue moddable) {
                moddable.onChange -= OnDependeeModdableChange;
                moddable.dependantModdables.Remove(this);
            }
            modifiers.Remove(foundMods[i]);
        }
        isDirty = true;
        onChange?.Invoke(Value, this);
        return true;
    }

    public virtual void OnDependeeModdableChange(float f, ModdableValue moddable) {
        isDirty = true;
        CalculateFinalValue();
    }

    public virtual bool ContainsModdableModifier(ModdableValue moddable) {
        var i = modifiers.FindIndex(x=>x.source == moddable);
        return i != -1;
    }



    public virtual void Reset() {
        baseValue = 0;
        isDirty = true;
        _value = 0;
        modifiers.Clear();
        onChange?.Invoke(Value, this);
    }

    public virtual void OnDestroy() {
        if(dependantModdables == null) return;
        for(int i = 0; i < dependantModdables.Count; ++i) {
            dependantModdables[i]?.RemoveModdableModifier(this);
        }
    }

    public float CalculateFinalValue() {
        float finalValue = baseValue;
        for(int i = 0; i < modifiers.Count; ++i) {
            finalValue = modifiers[i].ProcessValue(finalValue);
        }
        _value = finalValue;
        isDirty = false;
        onChange?.Invoke(_value, this);
        return _value;
    }

    public override string ToString() {
        return $"{id}: {Value}";
    }
}}