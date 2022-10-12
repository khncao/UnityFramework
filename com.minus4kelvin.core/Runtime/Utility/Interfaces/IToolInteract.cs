using UnityEngine;

namespace m4k {
public interface IToolInteract {
    Transform pivot { get; set; }
    void Init(bool visualize, bool applyState, GameObject owner, ScriptableObject item, int team = 0);
    void StartInteract(string toolId, Transform target);
    void StopInteract();
}
}