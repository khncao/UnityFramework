// ref: Unity multiplayer coop bossroom
using UnityEngine;

namespace m4k.Damage {
/// <summary>
/// Retrieved by damage dealers via TryGetComponent or GetComponent to call relevant methods. Generally there should only be one component that implements IDamageable interface per collider for predictable results.
/// </summary>
public interface IDamageable {
    Transform transform { get; }
    void ApplyDamage(int amount);
    void ApplyForce(float force, Vector3 hitPoint, Vector3 direction);
    bool IsDamageable { get; }
}
}