using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace m4k {
public class SerializableDictionaryTests {
    SerializableDictionary<int, int> dict;
    [SetUp]
    public void Setup() {
        dict = new SerializableDictionary<int, int>();
    }

    [Test]
    public void AddAndCountAndException() {
        dict.Add(0, 0);
        Assert.AreEqual(1, dict.Count);
        Assert.Catch(()=>dict.Add(0, 0), "Catch existing key");
        dict.Add(1, 1);
        Assert.AreEqual(2, dict.Count);
    }

    [Test]
    public void ThisAndTryGetValueAccessors() {
        dict.Add(12, 12);
        Assert.AreEqual(12, dict[12], "this[] accessor");
        Assert.True(dict.TryGetValue(12, out var val1), "TryGetValue accessor");
        Assert.AreEqual(12, val1, "TryGetValue correct value");
    }

    [Test]
    public void RemoveAndCountAndLimits() {
        dict.Add(0, 0);
        dict.Add(1, 1);
        Assert.AreEqual(2, dict.Count, "Correct dict length");
        Assert.False(dict.Remove(9), "False try remove nonexisting key");
        Assert.True(dict.Remove(1));
        Assert.False(dict.Remove(1), "False try remove nonexisting key");
        Assert.True(dict.Remove(0));
    }

    [Test]
    public void Clear() {
        dict.Add(0, 0);
        dict.Clear();
        Assert.False(dict.TryGetValue(0, out int val));
        dict.Add(1, 1);
        Assert.AreEqual(1, dict.Count);
        Assert.True(dict.TryGetValue(1, out int val2));
    }
}
}