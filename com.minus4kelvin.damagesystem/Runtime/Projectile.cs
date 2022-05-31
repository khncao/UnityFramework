// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Damage {
/// <summary>
/// Extendable base projectile.
/// </summary>
public class Projectile : MonoBehaviour, IPooledMonoBehaviour<Projectile>
{
    public int damage;
    public float force;
    public bool canHurtOwner;
    [Tooltip("Set to 0 for unlimited lifetime")]
    public float lifetime;

    // public int poolID { get; set; }
    public MonoBehaviourPooler<Projectile> pool { get; set; }
    public Transform owner { get; set; }
    public Vector3 target { get; private set; }

    public float projectedDeathTime { get; set; } = 0f; // time of expected death
    public float lastFireTime { get; set; } // time last fired
    public bool freeFlag { get; set; } // set true to free from manager

    public virtual void OnFire(Vector3 target, Transform owner) {
        this.target = target;
        this.owner = owner;
        lastFireTime = Time.time;
        if(lifetime > 0) {
            projectedDeathTime = Time.time + lifetime;
        }
    }

    /// <summary>
    /// Can be called from manager class such as RangeWeapon for additional time based logic. Probably best to call in FixedUpdate for physics alignment
    /// </summary>
    public virtual void OnUpdate() {

    }

    /// <summary>
    /// Free this projectile. Generally should set freeFlag to true and clean up in a manager that also handles lifetime freeing. Override to add logic before and after freeing
    /// </summary>
    public virtual void Free() {
        if(!freeFlag) {
            Debug.LogWarning($"Called Free on {gameObject} with unraised free flag");
        }
        freeFlag = false;
        pool.Free(this);
    }
}
}