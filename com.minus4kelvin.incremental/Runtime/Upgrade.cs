
using System.Collections.Generic;
using UnityEngine;
using m4k.ModdableValues;

namespace m4k.Incremental {
[System.Serializable]
public class UpgradeInstance {
    public string name;
    public long ownedAmount;
    
    [System.NonSerialized]
    Upgrade _upgrade;
    public Upgrade upgrade { get {
        if(_upgrade == null)
            _upgrade = IncrementalManager.I.GetUpgrade(name);
        return _upgrade;
    }}

    public UpgradeInstance(Upgrade upgrade) {
        this._upgrade = upgrade;
        this.name = upgrade.name;

        // workaround for SO retained states with enter playmode options
#if UNITY_EDITOR
        if(upgrade.costAmount != null)
            upgrade.costAmount = new ModdableValue(upgrade.costAmount.baseValue);
#endif
    }

    public long GetTotalQuantityCost(long quantity) {
        return (long)(quantity * upgrade.costAmount.Value);
    }

    public override string ToString() {
        return $"{upgrade.displayName}";
    }

    public string ToFullString() {
        System.Text.StringBuilder s = new System.Text.StringBuilder();

        s.AppendLine(upgrade.displayName);
        s.AppendLine(upgrade.description + '\n');
        s.AppendLine($"{ownedAmount} / {upgrade.maxLevel}");
        if(upgrade.costCurrency)
            s.AppendLine($"{upgrade.costAmount.Value} {upgrade.costCurrency.displayName} cost");

        return s.ToString();
    }
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "Data/Incremental/Upgrade", order = 0)]
public class Upgrade : ScriptableObject {
    public string displayName;
    public string description;
    public long maxLevel;
    [Range(0f, 1f)]
    public float perLevelDiminishPercent;

    [Header("Cost")]
    public Asset costCurrency;
    public ModdableValue costAmount;
}}