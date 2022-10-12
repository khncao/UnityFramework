using UnityEngine;
using System;

namespace m4k {
/// <summary>
/// Common interface for directable navigation agents. Used by AI, tasks, etc.
/// </summary>
public interface INavMovable {
    Transform Target { get; }
    float Speed { get; set; }
    bool IsMoving { get; }
    void SetTarget(Transform target);
    void SetTarget(Vector3 position);
    void SetFaceTarget(Transform target) => Debug.Log("Not implemented");
    void Stop();
    void Resume() => Debug.Log("Not implemented");
    void RegisterMovementBlocker(object obj) => Debug.Log("Not implemented");
    void UnregisterMovementBlocker(object obj) => Debug.Log("Not implemented");
    event Action OnArrive;
    event Action OnNewTarget;
}
}