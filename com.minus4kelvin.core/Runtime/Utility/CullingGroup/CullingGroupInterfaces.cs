
using UnityEngine;

namespace m4k {
/// <summary>
/// Root entity that is tracked by CullingGroup
/// </summary>
public interface ICullingGroupable {
    int cullingGroupIndex { get; set; }
    Transform transform { get; }
    GameObject gameObject { get; }
    void OnCullingGroupStateChange(CullingGroupEvent ev);
}

/// <summary>
/// Child components of ICullingGroupable that process CullingGroup state change events
/// </summary>
public interface ICullable {
    void OnCullingGroupStateChange(CullingGroupEvent ev);
}
}