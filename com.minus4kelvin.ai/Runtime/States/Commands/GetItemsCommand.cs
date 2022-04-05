using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
public struct GetItems : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public List<ItemInstance> items { get; private set; }
    StateInteractableInventory interactable;

    public GetItems(List<ItemInstance> items, StateInteractableInventory interactable, int priority = -1, StateProcessor processor = null) {
        this.items = items;
        this.interactable = interactable;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        // if(!StateInteractableManager.I.IsInteractableInRange(processor.transform, interactable))
        // {
        //     Debug.LogWarning("GetItems called outside of max interact dist");
        //     return;
        // }
        if(interactable == null) {
            interactable = StateInteractableManager.I.GetClosestInteractableInventoryWithItems(items, processor.transform);
        }
        if(interactable == null) {
            Debug.LogWarning("Failed to find suitable interactable inventory");
            return;
        }
        // processor.ShowItems(items);
        // processor.itemArranger.GetItems(items);
        processor.inventory?.inventory.AddItemAmounts(items);
        interactable.OnStateInteract(this);
    }

    public bool OnUpdate() => true;
    public void OnExit() { }
}

// [System.Serializable]
// public class GetItemsWrapper : StateWrapper {
//     public List<ItemInstance> items;


//     public override IState GetState() {
//         return new GetItems(items, )
//     }
// }
}