// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k.Damage {
/// <summary>
/// Example IDamageable implementation. 
/// </summary>
public class Damageable : MonoBehaviour, IDamageable
{
    [System.Serializable]
    public class DamageableEvents {
        public UnityEvent OnDeath, OnChangeHealth, OnResetDamage, OnHitWhileInvulnerable, OnBecomeVulnerable;
    }

    public int baseMaxHp = 100;

    public float invulnerabiltyTime;
    public DamageableEvents events; 

    public event System.Action<int> onDamaged;
    public event System.Action<float, Vector3, Vector3> onApplyForce;
    
    public int MaxHp { get; private set; }
    public int CurrentHp { get; private set; }
    public bool IsDead { get { return CurrentHp <= 0; }}
    public bool IsInvulnerable { get { return invulnerableTimerCR != null; }}

    float _timeLastDamaged;

    public bool IsDamageable { get { return IsDead || !IsInvulnerable; }}

    private void Awake() {
        MaxHp = baseMaxHp;
        CurrentHp = MaxHp;
    }

    public void ResetDamage(bool removeInvulnerability) {
        if(removeInvulnerability && invulnerableTimerCR != null) {
            StopCoroutine(invulnerableTimerCR);
            events.OnBecomeVulnerable.Invoke();
        }
        CurrentHp = MaxHp;
        events.OnResetDamage?.Invoke();
        events.OnChangeHealth?.Invoke();
    }

    public void ApplyDamage(int damage) {
        if(IsInvulnerable) {
            events.OnHitWhileInvulnerable.Invoke();
            return;
        }

        onDamaged?.Invoke(damage);
        TryAdjustHp(damage);
        _timeLastDamaged = Time.time;

        if(CurrentHp <= 0) {
            StartCoroutine(EndOfFrameDeath());
        }
        else if(invulnerabiltyTime > 0f)
            invulnerableTimerCR = StartCoroutine( InvulnerabilityTimer(invulnerabiltyTime) );
    }

    public void ApplyForce(float force, Vector3 hitPoint, Vector3 direction) {
        onApplyForce?.Invoke(force, hitPoint, direction);
    }

    IEnumerator EndOfFrameDeath() {
        yield return new WaitForEndOfFrame();
        events.OnDeath.Invoke();
    }

    Coroutine invulnerableTimerCR;
    IEnumerator InvulnerabilityTimer(float time) {
        yield return new WaitForSeconds(time);
        events.OnBecomeVulnerable.Invoke();
    }

    public bool TryAdjustHp(int amount, bool requireFullAmount = false) {
        var missingHp = MaxHp - CurrentHp;
        if(missingHp <= 0) {
            return false;
        }
        if(requireFullAmount && missingHp < amount) { // 
            return false;
        }
        
        CurrentHp += amount;
        events.OnChangeHealth.Invoke();
        return true;
    }

    public void SetMaxHp(int value) {
        MaxHp = value;
    }
}
}