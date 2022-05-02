
using System.Collections.Generic;
using UnityEngine;

namespace m4k.ModdableValues {
/// <summary>
/// Collection of related moddable values, such as character stats. Use prefab instances or component presets to easily duplicate a profile.
/// </summary>
public class ModdableValueGroup : MonoBehaviour {
    public List<ModdableValue> moddableValues;

    public void OnDestroy() {
        foreach(var m in moddableValues) {
            m.OnDestroy();
        }
    }
}}