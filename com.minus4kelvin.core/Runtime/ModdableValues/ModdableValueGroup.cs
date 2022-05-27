
using System.Collections.Generic;
using UnityEngine;

namespace m4k.ModdableValues {
/// <summary>
/// Collection of related moddable values, such as character stats. Use prefab instances or component presets to easily duplicate a profile.
/// </summary>
public class ModdableValueGroup : MonoBehaviour {
    [Tooltip("Runtime editor modifications do not affect internal dictionary. Programmatically add moddable values with accessors instead")]
    [SerializeField]
    List<ModdableValue> moddableValues;

    Dictionary<string, ModdableValue> moddableValuesDict = new Dictionary<string, ModdableValue>();

    private void Awake() {
        if(moddableValues == null) return;
        foreach(var i in moddableValues) {
            moddableValuesDict.Add(i.id, i);
        }
    }

    public void OnDestroy() {
        foreach(var m in moddableValuesDict) {
            m.Value.OnDestroy();
        }
    }

    // for testing
    public void Initialize() {
        if(moddableValues == null)
            moddableValues = new List<ModdableValue>();
        if(moddableValuesDict == null) 
            moddableValuesDict = new Dictionary<string, ModdableValue>();
    }

    public bool TryGetModdableValue(string moddableId, out ModdableValue moddableValue) {
        return moddableValuesDict.TryGetValue(moddableId, out moddableValue);
    }

    public bool AddModdableValue(ModdableValue moddable) {
        if(string.IsNullOrEmpty(moddable.id)) {
            Debug.LogError($"Moddable does not have id");
            return false;
        }
        if(moddableValuesDict.ContainsKey(moddable.id)) {
            Debug.LogError(gameObject.ToString() + $" already contains moddable with id: {moddable.id}");
            return false;
        }
        moddableValuesDict.Add(moddable.id, moddable);
        return true;
    }

    public bool RemoveModdableValue(string moddableId) {
        if(!moddableValuesDict.ContainsKey(moddableId)) {
            return false;
        }
        moddableValuesDict.Remove(moddableId);

        return true;
    }
}}