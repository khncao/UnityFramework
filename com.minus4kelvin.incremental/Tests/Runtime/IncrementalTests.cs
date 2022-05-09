using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using m4k.ModdableValues;

namespace m4k.Incremental {
public class IncrementalTests
{
    IncrementalManager incrementalManager;
    Currency currency1, currency2;
    Asset asset1, asset2, clickAsset;

    [SetUp]
    public void Setup() {
        var go = new GameObject();
        var gameTime = go.AddComponent<GameTime>();
        gameTime.timeProfile = new GameTime.TimeProfile();
        incrementalManager = go.AddComponent<IncrementalManager>();
        currency1 = ScriptableObject.CreateInstance<Currency>();
        currency1.name = "c1";
        currency2 = ScriptableObject.CreateInstance<Currency>();
        currency2.name = "c2";
        asset1 = ScriptableObject.CreateInstance<Asset>();
        asset1.name = "a1";
        asset2 = ScriptableObject.CreateInstance<Asset>();
        asset2.name = "a2";
        clickAsset = ScriptableObject.CreateInstance<Asset>();
        clickAsset.name = "click";
    }

    [Test]
    public void TestClick() {
        incrementalManager.AddCurrency(currency1);
        incrementalManager.TryGetOrCreateCurrencyInstance(currency1.name, out var currency1Instance);

        clickAsset.outputCurrency = currency1;
        clickAsset.outputAmount = new ModdableValue(3);
        incrementalManager.AssignClickAsset(clickAsset);
        incrementalManager.TransactAmount("click", 1); // 1 click per click

        Assert.AreEqual(0, (int)currency1Instance.ownedAmount);
        incrementalManager.Click();
        Assert.AreEqual(3, (int)currency1Instance.ownedAmount);
        for(int i = 0; i < 10; ++i) {
            incrementalManager.Click();
        }
        Assert.AreEqual(33, (int)currency1Instance.ownedAmount);
    }

    [Test]
    public void SerializeAndDeserialize() {
        // init and modify currency amount
        incrementalManager.AddCurrency(currency1);
        incrementalManager.TryGetOrCreateCurrencyInstance(currency1.name, out var savedC1);
        incrementalManager.TransactAmount(savedC1, 10);
        incrementalManager.TryGetOrCreateCurrencyInstance(currency1.name, out savedC1);
        Assert.AreEqual(10, (int)savedC1.ownedAmount, "Get modified currency instance");

        // asset amount
        incrementalManager.AddAsset(asset1);
        incrementalManager.TryGetOrCreateAssetInstance(asset1.name, out var savedA1);
        incrementalManager.TransactAmount(savedA1, 5);
        incrementalManager.TryGetOrCreateAssetInstance(asset1.name, out savedA1);
        Assert.AreEqual(5, (int)savedA1.ownedAmount, "Modify asset instance");

        // call serialize and deep copy to survive reset
        IncrementalSaveData shallowData = new IncrementalSaveData();
        IncrementalSaveData data = new IncrementalSaveData();
        incrementalManager.Serialize(ref shallowData);

        data.currencyInstances = new SerializableDictionary<string, CurrencyInstance>();
        data.assetInstances = new SerializableDictionary<string, AssetInstance>();
        var currencies = incrementalManager.GetCurrencyInstances();
        while(currencies.MoveNext()) {
            data.currencyInstances.Add(currencies.Current.Key, currencies.Current.Value);
        }

        var assets = incrementalManager.GetAssetInstances();
        while(assets.MoveNext()) {
            data.assetInstances.Add(assets.Current.Key, assets.Current.Value);
        }

        // call reset
        incrementalManager.Reset();

        // assert properly reset
        Assert.False(incrementalManager.TryGetOrCreateCurrencyInstance(currency1.name, out savedC1));

        // assert data intact
        Assert.AreEqual(10, (int)data.currencyInstances[currency1.name].ownedAmount, "Saved currency instance owned amount after serialize and reset");

        // deserialize data, setup, and assert data restored
        incrementalManager.Deserialize(data);

        incrementalManager.AddCurrency(currency1);
        incrementalManager.TryGetOrCreateCurrencyInstance(currency1.name, out var loadedC1);
        Assert.AreEqual(10, (int)loadedC1.ownedAmount);

        incrementalManager.AddAsset(asset1);
        incrementalManager.TryGetOrCreateAssetInstance(asset1.name, out var loadedA1);
        Assert.AreEqual(5, (int)loadedA1.ownedAmount);
    }
}
}