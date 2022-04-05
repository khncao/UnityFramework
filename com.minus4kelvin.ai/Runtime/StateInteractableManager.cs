using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using m4k.Items;

namespace m4k.AI {
// TODO: General IStateInteractable registry with key/type lookups

/// <summary>
/// Manages registry of active state interactables. States access through singleton pattern.
/// </summary>
public class StateInteractableManager : Singleton<StateInteractableManager>
{
    public TRegistry<StateInteractableInventory> inventoryInteractables;
    public TRegistry<IStateInteractable> stateInteractables;

    public float interactMaxSqrDist = 2.5f;

    Dictionary<string, List<StateInteractableKeyEvent>> keyTaskTargetsDict = new Dictionary<string, List<StateInteractableKeyEvent>>();



    public bool IsInteractableInRange(Transform t, IStateInteractable interactable) 
    {
        return (interactable.transform.position - t.position).sqrMagnitude < interactMaxSqrDist;
    }


    List<StateInteractableInventory> results = new List<StateInteractableInventory>();
    public StateInteractableInventory GetClosestInteractableInventoryWithItems(List<ItemInstance> items, Transform t) {
        results.Clear();
        StateInteractableInventory result = null;

        for(int i = 0; i < inventoryInteractables.instances.Count; ++i) 
        {
            if(inventoryInteractables.instances[i].HasItems(items))
                results.Add(inventoryInteractables.instances[i]);
        }
        t.GetClosest<StateInteractableInventory>(results, out result);
        return result;
    }



    public StateInteractableKeyEvent GetClosestInteractableKey(string key, Transform t) {
        List<StateInteractableKeyEvent> targets;
        if(!keyTaskTargetsDict.TryGetValue(key, out targets)) {
            Debug.LogWarning($"TaskTargets not found for {key}");
            return null;
        }
        StateInteractableKeyEvent closest = null;
        t.GetClosest<StateInteractableKeyEvent>(targets, out closest);
        return closest;
    }

    public void RegisterInteractableKey(string key, StateInteractableKeyEvent target) {
        List<StateInteractableKeyEvent> interactables;
        if(!keyTaskTargetsDict.TryGetValue(key, out interactables)) {
            interactables = new List<StateInteractableKeyEvent>();
            keyTaskTargetsDict.Add(key, interactables);
        }
        interactables.Add(target);
    }

    public void UnregisterInteractableKey(string key, StateInteractableKeyEvent target) {
        List<StateInteractableKeyEvent> interactables;
        if(!keyTaskTargetsDict.TryGetValue(key, out interactables)) {
            return;
        }
        interactables?.Remove(target);
    }

}
}