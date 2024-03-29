
using UnityEditor;
using UnityEngine;
using m4k.Items;

namespace m4k.BuildSystem {
[CreateAssetMenu(fileName = "ItemBuildable", menuName = "Data/Items/ItemBuildable")]
public class ItemBuildable : Item {
    // [Header("Buildable")]
    public override bool Primary(ItemSlotUI slot)
    {
        BuildingSystem.I.SetBuildObject(this);
        return true;
    }

    public override void AddToInventory(int amount, bool notify)
    {
        // base.AddToInventory(amount, notify);
        BuildingSystem.I.AddBuildItem(this, amount);
    }
    
}
}