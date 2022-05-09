using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace m4k.Incremental {
[Serializable]
public class IncrementalSaveData {
    public long saveTime;
    public SerializableDictionary<string, CurrencyInstance> currencyInstances;
    public SerializableDictionary<string, AssetInstance> assetInstances;
    public SerializableDictionary<string, UpgradeInstance> upgradeInstances;
}

public class IncrementalManager : Singleton<IncrementalManager> {
    // asset lookup
    // TODO: move to dependence asset manager
    [SerializeField, InspectInline]
    List<Currency> currencies;
    [SerializeField, InspectInline]
    List<Upgrade> upgrades;
    [SerializeField, InspectInline]
    List<Asset> assets;
    [SerializeField, InspectInline]
    Asset clickAsset;
    public IncrementalUIBase UIBase;

    public float globalCurrencyGainMultiplier = 1f;
    public bool processElapsedTimeSinceLastSave;

    // cache
    [NonSerialized]
    AssetInstance clickAssetInstance;
    [NonSerialized]
    CurrencyInstance clickOutputCurrencyInstance;

    public event Action onClick, onTick;
    public event Action<AssetInstance> onAssetChanged;
    public event Action<CurrencyInstance> onCurrencyChanged;
    public event Action<UpgradeInstance> onUpgradeChanged;
    public event Action<CurrencyInstance> onInitOrLoadCurrencyInstance;
    public event Action<AssetInstance> onInitOrLoadAssetInstance;
    public event Action<UpgradeInstance> onInitOrLoadUpgradeInstance;

    SerializableDictionary<string, CurrencyInstance> currencyInstanceDict = new SerializableDictionary<string, CurrencyInstance>();
    SerializableDictionary<string, AssetInstance> assetInstanceDict = new SerializableDictionary<string, AssetInstance>();
    SerializableDictionary<string, UpgradeInstance> upgradeInstanceDict = new SerializableDictionary<string, UpgradeInstance>();

    DateTime savedTime;
    TimeSpan elapsedTimeSinceLastSave;
    bool loadedFromSave;

    private IEnumerator Start() {
        GameTime.I.onTick -= Tick;
        GameTime.I.onTick += Tick;
        UIBase?.Initialize(this); // explicitly init UI for order

        if(currencies == null) currencies = new List<Currency>();
        if(assets == null) assets = new List<Asset>();
        if(upgrades == null) upgrades = new List<Upgrade>();
        
        yield return null; // delay for classes dependent on events in start
        
        if(loadedFromSave) { // if loaded from save
            ProcessElapsedTime();
            foreach(var i in currencyInstanceDict)
                onInitOrLoadCurrencyInstance?.Invoke(i.Value);
            foreach(var i in assetInstanceDict)
                onInitOrLoadAssetInstance?.Invoke(i.Value);
            foreach(var i in upgradeInstanceDict)
                onInitOrLoadUpgradeInstance?.Invoke(i.Value);
        }
        else { // create data instances for predefined currencies and assets
            foreach(var i in currencies)
                TryGetOrCreateCurrencyInstance(i.name, out var currencyInstance);
            foreach(var i in assets)
                TryGetOrCreateAssetInstance(i.name, out var assetInstance);
            foreach(var i in upgrades)
                TryGetOrCreateUpgradeInstance(i.name, out var upgradeInstance);
        }
        AssignClickAsset(clickAsset);
    }

    /// <summary>
    /// Parse output of all assets and add to designated output Currency
    /// </summary>
    /// <param name="multiplier"></param>
    void ProcessAssetOutputs(float multiplier) {
        foreach(var i in assetInstanceDict) {
            if(TryGetOrCreateCurrencyInstance(i.Value.asset.outputCurrency.name, out var currencyInstance)) {
                currencyInstance.ownedAmount 
                    += i.Value.GetTotalOutputAmount()
                    * (BigInteger)multiplier;
            }
        }
    }

    public void Tick(long tick) {
        ProcessAssetOutputs(globalCurrencyGainMultiplier);
        onTick?.Invoke();
    }

    public void Click() {
        if(clickAsset == null || clickAssetInstance == null) {
            Debug.LogWarning("Tried to click with no click asset");
            return;
        }
        clickOutputCurrencyInstance.ownedAmount 
            += clickAssetInstance.GetTotalOutputAmount() 
            * (BigInteger)globalCurrencyGainMultiplier;
        onClick?.Invoke();
    }

    // modify ownedAmount; accounts for currency cost

    public void TransactAmount(string id, long amount) {
        if(TryGetOrCreateAssetInstance(id, out var asset))
            TransactAmount(asset, amount);
        else if(TryGetOrCreateUpgradeInstance(id, out var upgrade))
            TransactAmount(upgrade, amount);
        else if(TryGetOrCreateCurrencyInstance(id, out var currency))
            TransactAmount(currency, amount);
        else if(id == clickAsset.name)
            TransactAmount(clickAssetInstance, amount);
    }

    public void TransactAmount(AssetInstance assetInstance, long amount) {
        var resultAmount = assetInstance.ownedAmount + amount;
        if(resultAmount < 0) {
            amount = (long)assetInstance.ownedAmount;
        }
        if(amount == 0) {
            Debug.LogWarning($"{assetInstance.name}; 0 transaction amount");
            return;
        }

        if(assetInstance.asset.costCurrency) {
            BigInteger totalCost = assetInstance.GetTotalQuantityCost(amount);

            if(!TryGetOrCreateCurrencyInstance(assetInstance.asset.costCurrency.name, out var costCurrencyInstance)) {
                Debug.Log($"{assetInstance.name} cost currency not found");
                return;
            }
            if(totalCost > costCurrencyInstance.ownedAmount) {
                Debug.Log($"{totalCost} total cost exceeds {costCurrencyInstance.name} owned amount {costCurrencyInstance.ownedAmount}");
                return;
            }
            costCurrencyInstance.ownedAmount -= totalCost;
        }
        assetInstance.ownedAmount += amount;
        onAssetChanged?.Invoke(assetInstance);
    }

    public void TransactAmount(UpgradeInstance upgradeInstance, long amount) {
        var resultAmount = amount + upgradeInstance.ownedAmount;
        if(resultAmount > upgradeInstance.upgrade.maxLevel || resultAmount < 0) {
            Debug.LogWarning($"Attempted to transact {amount} which would exceed {upgradeInstance.name} max level of {upgradeInstance.upgrade.maxLevel} or be below 0");
            return;
        }

        if(upgradeInstance.upgrade.costCurrency) {
            BigInteger totalCost = upgradeInstance.GetTotalQuantityCost(amount);

            if(!TryGetOrCreateCurrencyInstance(upgradeInstance.upgrade.costCurrency.name, out var costCurrencyInstance)) {
                Debug.Log($"{upgradeInstance.name} cost currency not found");
                return;
            }
            if(totalCost > costCurrencyInstance.ownedAmount) {
                Debug.Log($"{totalCost} total cost exceeds {costCurrencyInstance.name} owned amount {costCurrencyInstance.ownedAmount}");
                return;
            }
            costCurrencyInstance.ownedAmount -= totalCost;
        }
        upgradeInstance.ownedAmount += amount;
        onUpgradeChanged?.Invoke(upgradeInstance);
    }

    public void TransactAmount(CurrencyInstance currencyInstance, long amount) {
        currencyInstance.ownedAmount += amount;
        onCurrencyChanged?.Invoke(currencyInstance);
    }

    // Get or create data instances for currency, assets, upgrades; call init if create

    public bool TryGetOrCreateCurrencyInstance(string name, out CurrencyInstance currencyInstance) {
        if(!currencyInstanceDict.TryGetValue(name, out currencyInstance)) {
            var currency = GetCurrency(name);
            if(currency == null) {
                Debug.LogWarning($"{name} currency not found");
                return false;
            }
            currencyInstance = new CurrencyInstance(currency);
            currencyInstance.ownedAmount = (BigInteger)currency.startAmount;
            currencyInstanceDict.Add(name, currencyInstance);
            onInitOrLoadCurrencyInstance?.Invoke(currencyInstance);
        }
        return true;
    }

    public bool TryGetOrCreateAssetInstance(string name, out AssetInstance assetInstance) {
        if(!assetInstanceDict.TryGetValue(name, out assetInstance)) {
            var asset = GetAsset(name);
            if(asset == null) {
                Debug.LogWarning($"{name} asset not found");
                return false;
            }
            assetInstance = new AssetInstance(asset);
            assetInstance.ownedAmount = (BigInteger)asset.startAmount;
            assetInstanceDict.Add(name, assetInstance);
            onInitOrLoadAssetInstance?.Invoke(assetInstance);
        }
        return true;
    }

    public bool TryGetOrCreateUpgradeInstance(string name, out UpgradeInstance upgradeInstance) {
        if(!upgradeInstanceDict.TryGetValue(name, out upgradeInstance)) {
            var upgrade = GetUpgrade(name);
            if(upgrade == null) {
                Debug.LogWarning($"{name} upgrade not found");
                return false;
            }
            upgradeInstance = new UpgradeInstance(upgrade);
            upgradeInstanceDict.Add(name, upgradeInstance);
            onInitOrLoadUpgradeInstance?.Invoke(upgradeInstance);
        }
        return true;
    }
    
    // accessors

    public IEnumerator<KeyValuePair<string, CurrencyInstance>> GetCurrencyInstances() {
        return currencyInstanceDict.GetEnumerator();
    }

    public IEnumerator<KeyValuePair<string, AssetInstance>> GetAssetInstances() {
        return assetInstanceDict.GetEnumerator();
    }

    public Currency GetCurrency(string currencyName) {
        return currencies != null ? currencies.Find(x=>x.name == currencyName) : null;
    }

    public Asset GetAsset(string assetName) {
        return assets != null ? assets.Find(x=>x.name == assetName) : null;
    }

    public Upgrade GetUpgrade(string upgradeName) {
        return upgrades != null ? upgrades.Find(x=>x.name == upgradeName) : null;
    }

    // TODO: change to another asset management?

    public CurrencyInstance AddCurrency(Currency currency) {
        if(currencies == null) currencies = new List<Currency>();
        currencies.Add(currency);
        TryGetOrCreateCurrencyInstance(currency.name, out var c);
        return c;
    }

    public AssetInstance AddAsset(Asset asset) {
        if(assets == null) assets = new List<Asset>();
        assets.Add(asset);
        TryGetOrCreateAssetInstance(asset.name, out var a);
        return a;
    }
    

    public AssetInstance AssignClickAsset(Asset asset) {
        clickAsset = asset;

        if(asset == null) {
            Debug.LogWarning("Assigned null click asset");
            clickAssetInstance = null;
            clickOutputCurrencyInstance = null;
            return null;
        }
        
        if(!TryGetOrCreateAssetInstance(clickAsset.name, out clickAssetInstance) 
        && clickAssetInstance == null) {
            clickAssetInstance = new AssetInstance(clickAsset);
        }
        if(!TryGetOrCreateCurrencyInstance(clickAsset.outputCurrency.name, out clickOutputCurrencyInstance)) {
            Debug.LogWarning($"{clickAsset.ToString()} output currency not found");
        }
        return clickAssetInstance;
    }

    void ProcessElapsedTime() {
        Debug.Log("Elapsed time: " + elapsedTimeSinceLastSave.TotalSeconds.ToString());
        
        if(processElapsedTimeSinceLastSave) {
            float elapsedGameTicks = (float)(elapsedTimeSinceLastSave.TotalSeconds * GameTime.I.timeProfile.ticksPerSecond);
            Debug.Log($"Elapsed game ticks: {elapsedGameTicks}");
            ProcessAssetOutputs(elapsedGameTicks);
        }
    }

    /// <summary>
    /// Clear all state, including editor assigned assets and currencies. Method intended mainly for testing
    /// </summary>
    public void Reset() {
        clickAsset = null;
        clickAssetInstance = null;
        clickOutputCurrencyInstance = null;
        onAssetChanged = null;
        onCurrencyChanged = null;
        onUpgradeChanged = null;
        onInitOrLoadAssetInstance = null;
        onInitOrLoadCurrencyInstance = null;
        onInitOrLoadUpgradeInstance = null;
        onTick = null;
        onClick = null;
        currencies?.Clear();
        assets?.Clear();
        upgrades?.Clear();
        currencyInstanceDict.Clear();
        assetInstanceDict.Clear();
        upgradeInstanceDict.Clear();
    }

    // serialization

    public void Serialize(ref IncrementalSaveData data) {
        if(data == null) data = new IncrementalSaveData();
        data.saveTime = DateTime.Now.ToBinary();

        data.currencyInstances = currencyInstanceDict;
        data.assetInstances = assetInstanceDict;
        data.upgradeInstances = upgradeInstanceDict;
    }

    public void Deserialize(IncrementalSaveData data) {
        loadedFromSave = true;
        savedTime = DateTime.FromBinary(data.saveTime);
        elapsedTimeSinceLastSave = DateTime.Now.Subtract(savedTime);

        this.currencyInstanceDict = data.currencyInstances;
        this.assetInstanceDict = data.assetInstances;
        this.upgradeInstanceDict = data.upgradeInstances;
    }
}}