using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace m4k.Items {

public enum ItemTag {
    Consumable = 0, Drink = 1, Food = 2,
    Floor = 10, Light = 11, Table = 12, Seat = 13, Prop = 14, Zone = 15, Wall = 16, 
    Bar = 50, Kitchen = 51, Brew = 52, Foundry = 53, Stonemason = 54, Sawmill = 55,
    Hat = 60, Hairstyle = 61, Head = 62, Outfit = 63, Body = 64, Holdable = 65, RightHand = 66, LeftHand = 67,
    
}

/// <summary>
/// For inventory serialization
/// </summary>
[System.Serializable]
public struct ItemData {
    public string name;
    public int amount;
    public ItemData(string n, int a) {
        name = n;
        amount = a;
    }
}

/// <summary>
/// For inspector editable item and amounts; used as data for inventory slots, transfers, etc.
/// </summary>
[System.Serializable]
public class ItemInstance {
    public Item item;
    public int amount;

    [System.NonSerialized]
    public System.Action onChange;

    public string DisplayName { get { 
        return item ? item.displayName : "";
    }}

    public ItemInstance(Item i, int a) {
        item = i;
        amount = a;
    }
}

// [CreateAssetMenu(menuName="Data/Items/Item")]
[System.Serializable]
public class Item : ScriptableObject
{
    [SerializeField]
    string _displayName;
    public string description;

    [PreviewSpriteAttribute]
    public Sprite itemIcon;
    public List<ItemTag> itemTags;
    public AssetReferenceGameObject prefabRef;
    public int maxAmount = 1;
    public float value;

    // public GameObject prefab { get { return prefabRef.Asset as GameObject; }}

    /// <summary>
    /// Display name; label
    /// </summary>
    public string displayName {
        get {
            return !string.IsNullOrEmpty(_displayName) ? _displayName : name;
        }
        set {
            _displayName = value;
        }
    }

    HashSet<ItemTag> tagHash;

    public GameObject GetNewPrefabInstance(Vector3 position, Quaternion rotation, Transform parent = null) {
        if(!ValidPrefabReference()) {
            Debug.LogWarning($"{displayName} no prefab ref");
            return null;
        }
        var op = prefabRef.InstantiateAsync(position, rotation, parent);
        return op.WaitForCompletion();
    }

    public GameObject GetNewPrefabInstance(Transform parent = null, bool inWorldSpace = false) {
        if(!ValidPrefabReference()) {
            Debug.LogWarning($"{displayName} no prefab ref");
            return null;
        }
        var op = prefabRef.InstantiateAsync(parent, inWorldSpace);
        return op.WaitForCompletion();
    }

    public void ReleasePrefabInstance(GameObject go) {
        prefabRef.ReleaseInstance(go);
    }

    public bool HasTag(ItemTag tag) {
        if(tagHash == null) {
            tagHash = new HashSet<ItemTag>();
            foreach(var t in itemTags)
                tagHash.Add(t);
        }

        return tagHash.Contains(tag);
    }

    /// <summary>
    /// For input events such as primary/single click actions or CheckConditionsMet for ItemConditional
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public virtual bool Primary(ItemSlotUI slot) {
        return true;
    }

    /// <summary>
    /// For input events such as secondary/double click
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public virtual bool Secondary(ItemSlotUI slot) {
        return true;
    }

    /// <summary>
    /// For input events such as middle mouse/long press
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public virtual bool Tertiary(ItemSlotUI slot) {
        return true;
    }

    public virtual void AddToInventory(int amount, bool notify) {
        InventoryManager.I.mainInventory.AddItemAmount(this, amount, notify);
    }

    public virtual void ContextTransfer(ItemSlotUI slot) {
        InventoryManager.I.UI.InitiateItemTransfer(slot);
    }

    public virtual void Copy(Item item) {
        name = item.name;
        _displayName = item._displayName;
        description = item.description;
        prefabRef = item.prefabRef;
        maxAmount = item.maxAmount;
        value = item.value;
        itemIcon = item.itemIcon;
        itemTags = item.itemTags;
    }

    bool ValidPrefabReference() {
        var location = Addressables.LoadResourceLocationsAsync(prefabRef);
        if(location.WaitForCompletion().Count == 0)
            return false;
        return true;
    }
}
}