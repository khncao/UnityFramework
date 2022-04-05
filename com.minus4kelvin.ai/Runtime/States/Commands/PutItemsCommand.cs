using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
public struct PutItems : IState {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public List<ItemInstance> items { get; private set; }
    IStateInteractable interactable;

    public PutItems(List<ItemInstance> items, IStateInteractable interactable, int priority = -1, StateProcessor processor = null) {
        this.items = items;
        this.interactable = interactable;
        this.processor = processor;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        // if(!StateInteractableManager.I.IsInteractableInRange(processor.transform, interactable))
        // {
        //     Debug.LogWarning("GiveItems called outside of max interact dist");
        //     return;
        // }
        // if(interactable == null) {
        //     interactable = StateInteractableManager.I.GetClosestInteractableInventoryWithItems(items, processor.transform);
        // }
        if(interactable == null) {
            Debug.LogWarning("Failed to find suitable interactable inventory");
            return;
        }
        // processor.HideItems(items);
        // processor.itemArranger.RemoveItems(items);
        processor.inventory?.inventory.RemoveItemAmounts(items);
        interactable.OnStateInteract(this);
    }

    public bool OnUpdate() => true;
    public void OnExit() { }
} 

// [System.Serializable]
// public class PutItemsWrapper : StateWrapper {
//     public List<ItemInstance> items;


//     public override IState GetState() {
//         return new PutItems(items, )
//     }
// }
}