using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
[RequireComponent(typeof(InventoryComponent))]
public class StateInteractableInventory : MonoBehaviour, IStateInteractable
{
    public bool isQuantitive;
    [Tooltip("Temp test items")]
    public List<ItemInstance> items;

    Inventory _inventory;

    private void Start() {
        _inventory = GetComponent<InventoryComponent>().inventory;

        // foreach(var i in items)
        //     _inventory.AddItemAmount(i.item, i.amount);

        OnEnable();
    }

    private void OnEnable() {
        if(!StateInteractableManager.I) return;
        StateInteractableManager.I.inventoryInteractables?.RegisterInstance(this);
        StateInteractableManager.I.stateInteractables.RegisterInstance(this);
    }
    private void OnDisable() {
        if(!StateInteractableManager.I) return;
        StateInteractableManager.I.inventoryInteractables.UnregisterInstance(this);
        StateInteractableManager.I.stateInteractables.UnregisterInstance(this);
    }
    
    public void OnStateInteract(IState state) {
        if(!isQuantitive) {
            return;
        }
        if(state is GetItems getItems) {
            // transfer _inventory items to state.processor
            var transfer = getItems.items;
            foreach(var i in transfer) {
                _inventory.RemoveItemAmount(i.item, i.amount);
            }
        }
        else if(state is PutItems putItems) {
            // from state.processor to _inventory
            var transfer = putItems.items;
            foreach(var i in transfer) {
                _inventory.AddItemAmount(i.item, i.amount);
            }
        }
    }

    public bool CanStateInteract(IState state) {
        if(!isQuantitive) {
            return true;
        }
        if(state is GetItems getItems) {
            var transfer = getItems.items;
            foreach(var i in transfer) {
                if(!items.Exists(x=>x.item == i.item))
                    return false;
            }
        }
        else if(state is PutItems putItems) {
            var transfer = putItems.items;
            foreach(var i in transfer) {
                return _inventory.GetMaxAmountItemsFit(i.item) < i.amount;
            }
        }
        
        return false;
    }

    public bool HasItems(List<ItemInstance> items) {
        for(int i = 0; i < items.Count; ++i) {
            if(!this.items.Exists(x=>x.item == items[i].item))
                return false;
        }
        return true;
    }
}
}