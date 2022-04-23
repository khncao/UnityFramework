
using UnityEngine;

namespace m4k {
public class SavedSpawnObject : MonoBehaviour {
    public SpawnedObjectData data;

    public void UpdateSavedInstanceTransform() {
        data.pos = transform.position;
        data.rot = transform.rotation;
    }

    public void Destroy(bool deleteSavedObjectData) {
        SpawnManager.I?.FindDespawnInstance(gameObject, deleteSavedObjectData);
    }

    // private void OnDestroy() {
    //     SpawnManager.I?.FindDespawnInstance(gameObject);
    // }
}}