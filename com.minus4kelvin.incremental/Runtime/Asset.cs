
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using m4k.ModdableValues;

namespace m4k.Incremental {
[System.Serializable]
public class AssetInstance : CurrencyInstance {
    public string testString = "99";
    
    [System.NonSerialized]
    Asset _asset;
    public Asset asset { get {
        if(_asset == null)
            _asset = IncrementalManager.I.GetAsset(name);
        return _asset;
    }}

    public AssetInstance(Asset asset) : base(asset) {
        this._asset = asset;
        this.name = asset.name;

        // workaround for SO retained states with enter playmode options
#if UNITY_EDITOR
        if(asset.outputAmount != null)
            asset.outputAmount = new ModdableValue(asset.outputAmount.baseValue);
        if(asset.costAmount != null)
            asset.costAmount = new ModdableValue(asset.costAmount.baseValue);
#endif
    }

    public BigInteger GetTotalOutputAmount() {
        return ((BigInteger)asset.outputAmount.Value * ownedAmount);
    }

    public BigInteger GetTotalQuantityCost(long quantity) {
        return (BigInteger)(quantity * asset.costAmount.Value);
    }

    public BigInteger GetAffordQuantity(BigInteger amountCurrencyOwned) {
        if(asset.costAmount.Value <= 0) {
            Debug.LogWarning($"{asset.name} base cost is 0 or less");
            return 0;
        }
        if(amountCurrencyOwned < (BigInteger)asset.costAmount.Value)
            return 0;
        return (amountCurrencyOwned / (BigInteger)asset.costAmount.Value);
    }

    public override string ToString() {
        return $"{asset.displayName}";
    }
}

[CreateAssetMenu(fileName = "Asset", menuName = "Data/Incremental/Asset", order = 0)]
public class Asset : Currency {
    public bool hideInUI;
    [Header("Cost")]
    public Currency costCurrency;
    public ModdableValue costAmount;
    public Upgrade costUpgrade;
    [Header("Output")]
    public Currency outputCurrency;
    public ModdableValue outputAmount;
    public Upgrade outputUpgrade;
}}