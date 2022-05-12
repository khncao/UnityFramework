// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Damage {
public class Projectile : MonoBehaviour, IPooledMonoBehaviour<Projectile>, IDamager
{
    public int poolID { get; set; }
    public MonoBehaviourPooler<Projectile> pool { get; set; }
    public GameObject Self { get { return gameObject; }}
    public GameObject Owner { get; private set; }

    public virtual void Shot(Vector3 target, RangeWeapon shooter) {
        Owner = shooter.Owner;
    }

    public virtual void OnDamageDealt(Damageable damageable) {
        pool.Free(this);
    }
}
}