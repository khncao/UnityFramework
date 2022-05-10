
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
        incrementalManager.onAssetChanged += OnAssetChanged;
        incrementalManager.onUpgradeChanged += OnUpgradeChanged;
        incrementalManager.onInitOrLoadAssetInstance += OnInitOrLoadAsset;
        incrementalManager.onInitOrLoadUpgradeInstance += OnInitOrLoadUpgrade;
        incrementalManager.onTriggerRefresh += OnTriggerRefresh;
    }
    
    public virtual void OnDisable() {
        incrementalManager.onTick -= OnTick;
        incrementalManager.onClick -= OnClick;
        incrementalManager.onAssetChanged -= OnAssetChanged;
        incrementalManager.onUpgradeChanged -= OnUpgradeChanged;
        incrementalManager.onInitOrLoadAssetInstance -= OnInitOrLoadAsset;
        incrementalManager.onInitOrLoadUpgradeInstance -= OnInitOrLoadUpgrade;
        incrementalManager.onTriggerRefresh -= OnTriggerRefresh;
    }

    public abstract void OnTick();
    public abstract void OnClick();
    public abstract void OnAssetChanged(AssetInstance assetInstance);
    public abstract void OnUpgradeChanged(UpgradeInstance upgradeInstance);
    public virtual void OnInitOrLoadAsset(AssetInstance assetInstance) {}
    public virtual void OnInitOrLoadUpgrade(UpgradeInstance upgradeInstance) {}
    public virtual void OnTriggerRefresh() {}
    public virtual void OnSelectionChange(GameObject go) {}
}


/// <summary>
/// Example implementation of IncrementalUIBase
/// </summary>
public class IncrementalUI : IncrementalUIBase {
    public Transform currencyUIParent;
    public Transform assetUIParent;
    public GameObject currencyUIPrefab;
    public GameObject assetUIPrefab;

    public Transform upgradeUIParent;
    public GameObject upgradeUIPrefab;

    public TMP_Text currenciesText;
    public TMP_Text assetsText;
    public TMP_Text selectionDescriptionText;

    Dictionary<AssetInstance, AssetUI> assetInstantUIDict = new Dictionary<AssetInstance, AssetUI>();
    GameObject currentSelection;

    public override void OnTick() {
        UpdateAllAssets();
    }

    public override void OnClick() {
        OnTick();
    }


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
        assetUI.AssignAssetInstance(assetInstance, this);
        assetInstantUIDict.Add(assetInstance, assetUI);
    }

    public override void OnInitOrLoadUpgrade(UpgradeInstance upgradeInstance) {}

    public override void OnTriggerRefresh() {
        UpdateAllAssets();
        if(currentSelection)
            OnSelectionChange(currentSelection);
    }

    public override void OnSelectionChange(GameObject go) {
        currentSelection = go;
        if(!selectionDescriptionText) return;
        if(go.TryGetComponent<AssetUI>(out var assetUI)) {
            selectionDescriptionText.text = assetUI.currentAssetInstance.ToFullString();
        }
        else if(go.TryGetComponent<UpgradeUI>(out var upgradeUI)) {
            selectionDescriptionText.text = upgradeUI.currentUpgradeInstance.ToFullString();
        }
    }

    void UpdateAllAssets() {
        var assets = incrementalManager.GetAssetInstances();
        while(assets.MoveNext()) {
            var a = assets.Current;
            if(a.Value.asset.hideInUI) continue;
            OnAssetChanged(a.Value);
        }
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