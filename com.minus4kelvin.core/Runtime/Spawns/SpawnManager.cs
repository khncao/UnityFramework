using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace m4k {
[System.Serializable]
public class SpawnedObjectData {
    public string prefabKey;
    public Vector3 pos;
    public Quaternion rot;
    public string instanceGuid;

    [System.NonSerialized]
    public GameObject instance;

    public SpawnedObjectData(string prefabKey, GameObject instance) {
        this.prefabKey = prefabKey;
        this.pos = instance.transform.position;
        this.rot = instance.transform.rotation;
        this.instance = instance;
    }
}

[System.Serializable]
public class SpawnedObjectSavedData {
    public SerializableDictionary<string, List<SpawnedObjectData>> sceneSpawnedObjects;
}

/// <summary>
/// Manage saved spawned objects, cullable spawn points, etc.
/// </summary>
public class SpawnManager : Singleton<SpawnManager> {
    public bool despawnOnSceneChange;
    public List<SpawnedObjectData> currentSceneObjectData, previousSceneObjectData;

    SerializableDictionary<string, List<SpawnedObjectData>> sceneObjectData = new SerializableDictionary<string, List<SpawnedObjectData>>();
    Dictionary<GameObject, SpawnedObjectData> currentSceneObjectsDict = new Dictionary<GameObject, SpawnedObjectData>();

    HashSet<SpawnPoint> validSpawnPoints = new HashSet<SpawnPoint>();
    
    string currentSceneName;

    private void Start() {
        SceneHandler.I.onSceneChanged -= OnSceneChange;
        SceneHandler.I.onSceneChanged += OnSceneChange;
    }

    void OnSceneChange() {
        if(SceneHandler.I.isMainMenu) 
            return;

        if(UpdateCurrentSceneData()) {
            if(previousSceneObjectData != null)
                UpdateSavedInstanceData(previousSceneObjectData, despawnOnSceneChange);

            SpawnAndRestoreSavedInstances(currentSceneObjectData);
        }
    }

    bool UpdateCurrentSceneData() {
        if(!string.IsNullOrEmpty(currentSceneName) 
        && SceneHandler.I.activeScene.name == currentSceneName)
            return false;

        currentSceneName = SceneHandler.I.activeScene.name;
        if(string.IsNullOrEmpty(currentSceneName))
            return false;

        previousSceneObjectData = currentSceneObjectData;
        
        if(sceneObjectData == null) {
            Debug.LogWarning("Null sceneObjectData");
            sceneObjectData = new SerializableDictionary<string, List<SpawnedObjectData>>();
        }
        
        if(!sceneObjectData.TryGetValue(currentSceneName, out currentSceneObjectData)) 
        {
            currentSceneObjectData = new List<SpawnedObjectData>();
            sceneObjectData.Add(currentSceneName, currentSceneObjectData);
            currentSceneObjectsDict.Clear();
        }
        foreach(var data in currentSceneObjectData) {
            currentSceneObjectsDict.Add(data.instance, data);
        }
        return true;
    }

    public GameObject SpawnInstance(AssetReference assetReference) {
        var op = assetReference.InstantiateAsync();
        GameObject instance = op.WaitForCompletion();

        NewSpawnedObjectData(assetReference.AssetGUID, instance);

        return instance;
    }

    public GameObject SpawnInstance(string prefabKey) {
        GameObject instance = null;

        if(!TryGetInstance(prefabKey, out instance)) 
            return null;

        NewSpawnedObjectData(prefabKey, instance);

        return instance;
    }

    public void FindDespawnInstance(GameObject instance, bool deleteSavedData) {
        if(!currentSceneObjectsDict.ContainsKey(instance)) {
            Debug.LogWarning("Instance not found");
            return;
        }
        var data = currentSceneObjectsDict[instance];
        if(data != null) {
            if(!Addressables.ReleaseInstance(instance))
                Destroy(data.instance);

            if(deleteSavedData) {
                currentSceneObjectData.Remove(data);
                currentSceneObjectsDict.Remove(instance);
            }
        }
        Debug.Log($"Despawned {instance}");
    }

    public bool TryGetInstance(string prefabKey, out GameObject instance) {
        instance = null;
        var location = Addressables.LoadResourceLocationsAsync(prefabKey, typeof(GameObject));
        if(location.WaitForCompletion().Count == 0)
            return false;
        // var op = Addressables.LoadAssetAsync<GameObject>(location.Result[0]);
        var op = Addressables.InstantiateAsync(location.Result[0]);
        instance = op.WaitForCompletion();
        return instance;
    }

    void SpawnAndRestoreSavedInstances(List<SpawnedObjectData> data) {
        for(int i = 0; i < data.Count; ++i) {
            if(TryGetInstance(data[i].prefabKey, out var prefab)) {
                data[i].instance = Instantiate(prefab);
            }
            else {
                Debug.LogWarning($"{data[i].prefabKey} prefab not found");
                return;
            }

            data[i].instance.transform.position = data[i].pos;
            data[i].instance.transform.rotation = data[i].rot;

            var guidComponent = data[i].instance.GetComponentInChildren<GuidComponent>();
            if(guidComponent) {
                guidComponent.SetGuid(data[i].instanceGuid);
            }
        }
    }

    void UpdateSavedInstanceData(List<SpawnedObjectData> data, bool destroyInstances = false) {
        foreach(var d in data) {
            if(!d.instance) {
                Debug.LogWarning($"{d.prefabKey} data instance missing");
                continue;
            }
            d.pos = d.instance.transform.position;
            d.rot = d.instance.transform.rotation;

            if(destroyInstances)
                if(!Addressables.ReleaseInstance(d.instance))
                    Destroy(d.instance);
        }
    }

    SpawnedObjectData NewSpawnedObjectData(string prefabKey, GameObject instance) {
        SpawnedObjectData data = new SpawnedObjectData(prefabKey, instance);

        var guidComponent = instance.GetComponentInChildren<GuidComponent>();
        if(guidComponent)
            data.instanceGuid = guidComponent.GetGuid().ToString();

        if(!instance.TryGetComponent<SavedSpawnObject>(out var spawnedObject)) {
            spawnedObject = instance.AddComponent<SavedSpawnObject>();
            spawnedObject.data = data;
        }

        if(currentSceneObjectData == null)
            UpdateCurrentSceneData();

        currentSceneObjectData.Add(data);
        currentSceneObjectsDict.Add(instance, data);
        
        return data;
    }
    
    // Spawn points
    Dictionary<SpawnPoint, int> spawnPoints = new Dictionary<SpawnPoint, int>();

    public void RegisterValidSpawnPoint(SpawnPoint spawn) {
        if(!validSpawnPoints.Contains(spawn))
            validSpawnPoints.Add(spawn);
    }

    public void UnregisterValidSpawnPoint(SpawnPoint spawn) {
        validSpawnPoints.Remove(spawn);
    }

    public IEnumerator<SpawnPoint> GetValidSpawnPointEnumerator() {
        return validSpawnPoints.GetEnumerator();
    }

    // public bool TryGetSpawnPointOnNavMesh(Vector3 position) {
    //     Vector3 newPosition = transform.position;
    //     if(NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas)) {
    //         position = hit.position;
    //         return true;
    //     }
    //     return false;
    // }

    // Serialization

    public void Serialize(ref SpawnedObjectSavedData data) {
        if(data == null) data = new SpawnedObjectSavedData();
        foreach(var d in sceneObjectData)
            UpdateSavedInstanceData(d.Value);

        data.sceneSpawnedObjects = sceneObjectData;
    }

    public void Deserialize(SpawnedObjectSavedData data) {
        sceneObjectData = data.sceneSpawnedObjects;
    }
}
}