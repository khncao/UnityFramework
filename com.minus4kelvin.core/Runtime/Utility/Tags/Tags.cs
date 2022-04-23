
using System.Collections.Generic;

namespace m4k {
[System.Serializable]
public class Tags {
    public List<string> tags = new List<string>();

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
}
}