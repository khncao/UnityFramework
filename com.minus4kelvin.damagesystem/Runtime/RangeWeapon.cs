// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.Damage {
// TODO: extendable base projectile shooter class
public class RangeWeapon : MonoBehaviour
{
    public Vector3 muzzleOffset;
    public Projectile projectile;
    public RandomAudioPlayer attackAudio;
    public int damage;
    public float force;
    public int preloadProjectileCount = 20;
    public Transform owner { get; set; }

    public Projectile loadedProjectile {
        get { return m_LoadedProjectile; }
    }

    protected Projectile m_LoadedProjectile = null;
    protected MonoBehaviourPooler<Projectile> m_ProjectilePool;
    protected List<Projectile> activeProjectiles = new List<Projectile>();

    private void Start()
    {
        m_ProjectilePool = new MonoBehaviourPooler<Projectile>(preloadProjectileCount, projectile);
    }

    private void FixedUpdate() {
        // free projectile if flagged or past projectile lifetime
        for(int i = activeProjectiles.Count; i >= 0; --i) {
            if(activeProjectiles[i] == null) 
                continue;

            activeProjectiles[i].OnUpdate();

            if(activeProjectiles[i].freeFlag
                || (activeProjectiles[i].projectedDeathTime > 0f && activeProjectiles[i].projectedDeathTime > Time.time)) 
            {
                activeProjectiles[i].Free();
                activeProjectiles.RemoveAt(i);
            }
        }
    }

    public void Attack(Vector3 target)
    {
        AttackProjectile(target);
    }

    public void LoadProjectile()
    {
        if (m_LoadedProjectile != null)
            return;

        m_LoadedProjectile = m_ProjectilePool.GetNew();
        m_LoadedProjectile.transform.SetParent(transform, false);
        m_LoadedProjectile.transform.localPosition = muzzleOffset;
        m_LoadedProjectile.transform.localRotation = Quaternion.identity;
    }

    void AttackProjectile(Vector3 target)
    {
        if (m_LoadedProjectile == null) LoadProjectile();

        m_LoadedProjectile.transform.SetParent(null, true);
        m_LoadedProjectile.OnFire(target, owner);
        activeProjectiles.Add(m_LoadedProjectile);
        m_LoadedProjectile = null; 
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 worldOffset = transform.TransformPoint(muzzleOffset);
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawLine(worldOffset + Vector3.up * 0.4f, worldOffset + Vector3.down * 0.4f);
        UnityEditor.Handles.DrawLine(worldOffset + Vector3.forward * 0.4f, worldOffset + Vector3.back * 0.4f);
    }
#endif
}
}
