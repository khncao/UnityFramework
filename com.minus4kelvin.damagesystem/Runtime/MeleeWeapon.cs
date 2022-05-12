// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace m4k.Damage {
public class MeleeWeapon : MonoBehaviour, IDamager
{
    [System.Serializable]
    public class AttackPoint
    {
        public float radius;
        public Vector3 offset;
        public Transform attackRoot;

#if UNITY_EDITOR
        //editor only as it's only used in editor to display the path of the attack that is used by the raycast
        [NonSerialized] public List<Vector3> previousPositions = new List<Vector3>();
#endif
    }

    public int damage = 1;
    public float force = 10f;
    public ParticleSystem hitParticlePrefab;
    public LayerMask targetLayers;
    public AttackPoint[] attackPoints = new AttackPoint[0];

    [Header("Audio")]
    public RandomAudioPlayer hitAudio;
    public RandomAudioPlayer attackAudio;

    public GameObject Self { get { return gameObject; }}
    public GameObject Owner { get; private set; }

    protected Vector3[] m_PreviousPos = null;
    protected Vector3 m_Direction;

    protected bool m_InAttack = false;

    const int PARTICLE_COUNT = 10;
    protected ParticleSystem[] m_ParticlesPool = new ParticleSystem[PARTICLE_COUNT];
    protected int m_CurrentParticle = 0;

    protected static RaycastHit[] s_RaycastHitCache = new RaycastHit[32];
    protected static Collider[] s_ColliderCache = new Collider[32];

    private void Awake() {
        if (hitParticlePrefab != null)
        {
            for (int i = 0; i < PARTICLE_COUNT; ++i)
            {
                m_ParticlesPool[i] = Instantiate(hitParticlePrefab);
                m_ParticlesPool[i].Stop();
            }
        }
    }

    //whoever own the weapon is responsible for calling that. Allow to avoid "self harm"
    public void SetOwner(GameObject owner) {
        Owner = owner;
    }

    public void BeginAttack(bool thowingAttack)
    {
        if (attackAudio != null)
            attackAudio.PlayRandomClip();

        m_InAttack = true;
        m_PreviousPos = new Vector3[attackPoints.Length];

        for (int i = 0; i < attackPoints.Length; ++i)
        {
            Vector3 worldPos = attackPoints[i].attackRoot.position +
                                attackPoints[i].attackRoot.TransformVector(attackPoints[i].offset);
            m_PreviousPos[i] = worldPos;

#if UNITY_EDITOR
            attackPoints[i].previousPositions.Clear();
            attackPoints[i].previousPositions.Add(m_PreviousPos[i]);
#endif
        }
    }

    public void EndAttack()
    {
        m_InAttack = false;

#if UNITY_EDITOR
        for (int i = 0; i < attackPoints.Length; ++i)
            attackPoints[i].previousPositions.Clear();
#endif
    }

    private void FixedUpdate()
    {
        if (m_InAttack)
        {
            for (int i = 0; i < attackPoints.Length; ++i)
            {
                AttackPoint pts = attackPoints[i];

                Vector3 worldPos = pts.attackRoot.position + pts.attackRoot.TransformVector(pts.offset);
                Vector3 attackVector = worldPos - m_PreviousPos[i];

                if (attackVector.sqrMagnitude < 0.0001f) {
                    attackVector = Vector3.forward * 0.0001f;
                }
                Ray r = new Ray(worldPos, attackVector.normalized);

                int contacts = Physics.SphereCastNonAlloc(r, pts.radius, s_RaycastHitCache, attackVector.sqrMagnitude, ~0, QueryTriggerInteraction.Ignore);

                for (int k = 0; k < contacts; ++k) {
                    if (s_RaycastHitCache[k].collider != null)
                        CheckDamage(s_RaycastHitCache[k], pts);
                }

                m_PreviousPos[i] = worldPos;

#if UNITY_EDITOR
                pts.previousPositions.Add(m_PreviousPos[i]);
#endif
            }
        }
    }

    private bool CheckDamage(RaycastHit hit, AttackPoint pts)
    {
        Collider col = hit.collider;

        if((targetLayers.value & (1 << col.gameObject.layer)) == 0) {
            return false;
        }

        Damageable d = col.GetComponent<Damageable>();
        if(d && d.gameObject == Owner) {
            return true;
        }

        if (hitAudio != null) {
            var renderer = col.GetComponent<Renderer>();
            if (!renderer)
                renderer = col.GetComponentInChildren<Renderer> ();
            if (renderer)
                hitAudio.PlayRandomClip (renderer.sharedMaterial);
            else
                hitAudio.PlayRandomClip ();
        }
        if(!d) {
            return false;
        }

        DamageMessage data = new DamageMessage();

        data.amount = damage;
        data.force = force;
        data.damager = this;
        data.hitPoint = hit.point;
        data.direction = m_Direction.normalized;

        d.ApplyDamage(data);

        if (hitParticlePrefab != null)
        {
            m_ParticlesPool[m_CurrentParticle].transform.position = pts.attackRoot.transform.position;
            m_ParticlesPool[m_CurrentParticle].time = 0;
            m_ParticlesPool[m_CurrentParticle].Play();
            m_CurrentParticle = (m_CurrentParticle + 1) % PARTICLE_COUNT;
        }

        return true;
    }

    public void OnDamageDealt(Damageable damageable) {

    }

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < attackPoints.Length; ++i)
        {
            AttackPoint pts = attackPoints[i];

            if (pts.attackRoot != null)
            {
                Vector3 worldPos = pts.attackRoot.TransformVector(pts.offset);
                Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
                Gizmos.DrawSphere(pts.attackRoot.position + worldPos, pts.radius);
            }

            if (pts.previousPositions.Count > 1)
            {
                UnityEditor.Handles.DrawAAPolyLine(10, pts.previousPositions.ToArray());
            }
        }
    }

#endif
}
}