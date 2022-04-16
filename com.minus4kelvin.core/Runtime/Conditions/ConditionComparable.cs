using System;
using UnityEngine;

namespace m4k {
[Serializable]
public class ConditionComparable<T, T1> : Condition 
    where T : IComparable 
    where T1 : PrimitiveBaseSO<T> 
{
    public string description;
    public T1 obj;
    public ComparisonType op;
    public T val;

    public override bool CheckConditionMet() {
        if(!obj) {
            Debug.LogError("No obj in condition");
            return false;
        }
        return Comparisons.Compare(op, obj.value, val);
    }

    public override string ToString() {
        if(!obj) {
            Debug.LogError("No obj in condition");
            return "";
        }
        bool pass = Comparisons.Compare(op, obj.value, val);

        string col = pass ? "green" : "white";
        return $"<color={col}>- {description}: {obj.value}/{val}</color>";
    }

    public override void RegisterListener(Conditions conditions) {
        obj.onChange -= conditions.OnChange;
        obj.onChange += conditions.OnChange;
    }

    public override void UnregisterListener(Conditions conditions) {
        obj.onChange -= conditions.OnChange;
    }
}

[Serializable]
public class ConditionIntComparable : ConditionComparable<int, IntSO> {
}

[Serializable]
public class ConditionFloatComparable : ConditionComparable<float, FloatSO> { }

[Serializable]
public class ConditionBoolComparable : ConditionComparable<bool, BoolSO> { }
}