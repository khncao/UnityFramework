
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace m4k.Incremental {
/// <summary>
/// Optional UI base class
/// </summary>
public abstract class IncrementalUIBase : MonoBehaviour {
    protected IncrementalManager incrementalManager;

    public virtual void Initialize(IncrementalManager manager) {
        this.incrementalManager = manager;
        OnTick();
        OnEnable();
    }

    public virtual void OnEnable() {
        if(incrementalManager == null) return;
        OnDisable();
        incrementalManager.onTick += OnTick;
        incrementalManager.onClick += OnClick;
        incrementalManager.onCurrencyChanged += OnCurrencyChanged;
        incrementalManager.onAssetChanged += OnAssetChanged;
        incrementalManager.onUpgradeChanged += OnUpgradeChanged;
        incrementalManager.onInitOrLoadCurrencyInstance += OnInitOrLoadCurrency;
        incrementalManager.onInitOrLoadAssetInstance += OnInitOrLoadAsset;
        incrementalManager.onInitOrLoadUpgradeInstance += OnInitOrLoadUpgrade;
    }
    
    public virtual void OnDisable() {
        incrementalManager.onTick -= OnTick;
        incrementalManager.onClick -= OnClick;
        incrementalManager.onCurrencyChanged -= OnCurrencyChanged;
        incrementalManager.onAssetChanged -= OnAssetChanged;
        incrementalManager.onUpgradeChanged -= OnUpgradeChanged;
        incrementalManager.onInitOrLoadCurrencyInstance -= OnInitOrLoadCurrency;
        incrementalManager.onInitOrLoadAssetInstance -= OnInitOrLoadAsset;
        incrementalManager.onInitOrLoadUpgradeInstance -= OnInitOrLoadUpgrade;
    }

    public abstract void OnTick();
    public abstract void OnClick();
    public abstract void OnCurrencyChanged(CurrencyInstance currencyInstance);
    public abstract void OnAssetChanged(AssetInstance assetInstance);
    public abstract void OnUpgradeChanged(UpgradeInstance upgradeInstance);
    public virtual void OnInitOrLoadCurrency(CurrencyInstance currencyInstance) {}
    public virtual void OnInitOrLoadAsset(AssetInstance assetInstance) {}
    public virtual void OnInitOrLoadUpgrade(UpgradeInstance upgradeInstance) {}
}


/// <summary>
/// Example implementation of IncrementalUIBase
/// </summary>
public class IncrementalUI : IncrementalUIBase {
    public Transform currencyUIParent;
    public Transform assetUIParent;
    public GameObject currencyUIPrefab;
    public GameObject assetUIPrefab;

    public TMP_Text currenciesText;
    public TMP_Text assetsText;

    Dictionary<AssetInstance, AssetUI> assetInstantUIDict = new Dictionary<AssetInstance, AssetUI>();

    public override void OnTick() {
        UpdateAllCurrencyText();
    }

    public override void OnClick() {
        OnTick();
    }

    public override void OnCurrencyChanged(CurrencyInstance currencyInstance) {}

    public override void OnAssetChanged(AssetInstance assetInstance) {
        if(assetInstance == null) {
            UpdateAllAssetText();
        }
        else if(assetInstantUIDict.TryGetValue(assetInstance, out var assetUI)) {
            assetUI.UpdateUI();
        }
        else {
            Debug.LogWarning($"{assetInstance.asset.displayName} not found");
        }
    }

    public override void OnUpgradeChanged(UpgradeInstance upgradeInstance) {}

    public override void OnInitOrLoadCurrency(CurrencyInstance currencyInstance) {
        if(currencyUIPrefab == null || currencyUIParent == null)
            return;
    }

    public override void OnInitOrLoadAsset(AssetInstance assetInstance) {
        if(assetUIPrefab == null || assetUIParent == null) {
            Debug.LogWarning("Missing asset UI prefab or parent transform");
            return;
        }
        if(assetInstance.asset.hideInUI) {
            return;
        }
        if(assetInstantUIDict.ContainsKey(assetInstance)) {
            Debug.LogWarning($"{assetInstance.ToString()} already initialized UI");
            return;
        }
        var instance = Instantiate(assetUIPrefab, assetUIParent);
        if(!instance.TryGetComponent<AssetUI>(out var assetUI)) {
            Debug.LogWarning("AssetUI component not found on asset UI prefab root");
            return;
        }
        assetUI.AssignAssetInstance(assetInstance);
        assetInstantUIDict.Add(assetInstance, assetUI);
    }

    public override void OnInitOrLoadUpgrade(UpgradeInstance upgradeInstance) {}

    void UpdateAllCurrencyText() {
        if(!currenciesText) return;
        System.Text.StringBuilder s = new System.Text.StringBuilder();

        var currencies = incrementalManager.GetCurrencyInstances();
        while(currencies.MoveNext()) {
            var c = currencies.Current;
            s.Append($"{c.Value.ToString()} {c.Value.ownedAmount}\n");
        }
        currenciesText.text = s.ToString();
    }

    void UpdateAllAssetText() {
        if(!assetsText) return;
        System.Text.StringBuilder s = new System.Text.StringBuilder("Assets\n");
        
        var assets = incrementalManager.GetAssetInstances();
        while(assets.MoveNext()) {
            var a = assets.Current;
            if(a.Value.asset.hideInUI) continue;
            s.Append($"{a.Value.ToString()} {a.Value.ownedAmount}\n");
        }

        assetsText.text = s.ToString();
    }
}}