
using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public class GameObjectRegistryHandler : MonoBehaviour {
    public List<GameObjectListSO> registries;

    private void Awake() {
        foreach(var registry in registries) {
            registry.Add(gameObject);
        }
    }

    private void OnDestroy() {
        foreach(var registry in registries) {
            registry.Remove(gameObject);
        }
    }
}
}