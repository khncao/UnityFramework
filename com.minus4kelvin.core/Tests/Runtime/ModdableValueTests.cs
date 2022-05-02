using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace m4k.ModdableValues {
public class ModdableValueTests
{
    ModdableValue dmgStat, strStat, dexStat;
    ValueMod percentMultMod, percentAddMod;

    [SetUp]
    public void Setup() {
        dmgStat = new ModdableValue(0f, "dmg");
        strStat = new ModdableValue(100f, "str");
        dexStat = new ModdableValue(50f, "dex");
        percentMultMod = new ValueMod(0f, null, 0.1f);
        percentAddMod = new ValueMod(0f, null, 0.2f);
    }

    [Test]
    public void InitializeAndSetBase() {
        Assert.AreEqual(dmgStat.Value, 0f);
        dmgStat.SetBaseValue(1f);
        Assert.AreEqual(dmgStat.Value, 1f);
    }

    [Test]
    public void Reset() {
        InitializeAndSetBase();
        dmgStat.Reset();
        Assert.AreEqual(dmgStat.Value, 0f);
        AddModdableDependency();
        dmgStat.Reset();
        Assert.AreEqual(dmgStat.Value, 0f);
    }

    [Test]
    public void AddModdableDependency() {
        dmgStat.AddModdableModifier(strStat);
        Assert.AreEqual(dmgStat.Value, 100f);
    }

    [Test]
    public void PercentMultModifier() {
        dmgStat.SetBaseValue(100f);
        dmgStat.AddModifier(percentMultMod);
        Assert.AreEqual(dmgStat.Value, 110f);
    }

    [Test]
    public void PercentAddModifier() {
        dmgStat.SetBaseValue(100f);
        dmgStat.AddModifier(percentAddMod);
        Assert.AreEqual(dmgStat.Value, 120f);
    }

    [Test]
    public void ChangedModdableDependency() {
        dmgStat.AddModdableModifier(strStat);
        strStat.AddModdableModifier(dexStat);
        Assert.AreEqual(dmgStat.Value, strStat.Value);
    }

    [Test]
    public void RemoveModdable() {
        AddModdableDependency();
        dmgStat.RemoveModdableModifier(strStat);
        Assert.AreEqual(dmgStat.Value, 0f);
    }

    [Test]
    public void TestRemoveAllSource() {
        AddModdableDependency();
        dmgStat.RemoveAllModifiersFromSource(strStat);
        Assert.AreEqual(dmgStat.Value, 0f);
    }

    [UnityTest]
    public IEnumerator DestroyedModdableValueProperDisposal() {
        dmgStat = new ModdableValue(0f, "dmg");
        strStat = new ModdableValue(100f, "str");
        dmgStat.AddModdableModifier(strStat);
        Assert.AreEqual(dmgStat.Value, strStat.Value);

        var go = new GameObject();
        var moddableGroup = go.AddComponent<ModdableValueGroup>();
        moddableGroup.moddableValues = new List<ModdableValue>();

        moddableGroup.moddableValues.Add(strStat);
        GameObject.Destroy(go);

        yield return null;
        Assert.AreEqual(dmgStat.Value, 0f);
    }

}
}