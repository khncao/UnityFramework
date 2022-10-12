
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m4k {
[CreateAssetMenu(fileName = "TagsSO", menuName = "Data/TagsSO", order = 0)]
/// <summary>
/// Persistent collection of tags. Used as database and for runtime tag validation. Accessible as singleton through static I variable
/// </summary>
public class TagsSO : ScriptableObject {
    public static TagsSO I {
        get {
#if UNITY_EDITOR
            if(!_instance) _instance = GetAsset();
#endif
            return _instance;
        }
    }
    static TagsSO _instance;

    public List<string> tags;
    
    HashSet<string> tagsHash;

    public void AddTag(string tag) {
        if(ContainsTag(tag)) return;
        tags.Add(tag);
        tagsHash.Add(tag);
    }

    public void RemoveTag(string tag) {
        if(!ContainsTag(tag)) return;
        tags.Remove(tag);
        tagsHash.Remove(tag);
    }

    public bool ContainsTag(string value) {
        if(tagsHash == null) {
            tagsHash = new HashSet<string>();
            foreach(var t in tags)
                tagsHash.Add(t);
        }
        else if(tagsHash.Count != tags.Count) {
            tagsHash.Clear();
            foreach(var t in tags)
                tagsHash.Add(t);
        }
        return tagsHash.Contains(value);
    }

    public void Reset() {
        tagsHash.Clear();
    }

    private void OnEnable() {
        _instance = this;
    }

    /// <summary>
    /// Validate tag during use: string testTag = TagsSO.I.ValidTag(tag);
    /// </summary>
    /// <param name="tag"></param>
    /// <returns></returns>
    public string ValidTag(string tag) {
        if(ContainsTag(tag))
            return tag;
        Debug.LogWarning($"Invalid tag: {tag}");
        return "";
    }

#if UNITY_EDITOR
// Store at: Assets/Data/Profiles or any Resources folder as "TagsSO"
    public static TagsSO GetAsset() {
        var guids = AssetDatabase.FindAssets("t:TagsSO", new string[]{"Assets/Data/Profiles"});
        if(guids.Length < 1) {
            var tagsSO = Resources.Load<TagsSO>("TagsSO");

            if(tagsSO == null) {
                Debug.LogError("TagsSO not found");
                return null;
            }
            return tagsSO;
        }
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<TagsSO>(path);
    }
#endif
}}