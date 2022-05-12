// Adapted from: https://github.com/Unity-Technologies/Gamekit3D

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace m4k.Damage {
public interface IDamager {
    GameObject Self{ get; }
    GameObject Owner{ get; }
    void OnDamageDealt(Damageable damageable);
}

public struct DamageMessage 
{
    public IDamager damager;
    public int amount;
    public float force;
    public Vector3 hitPoint;
    public Vector3 direction;
}

public class Damageable : MonoBehaviour 
{
    [System.Serializable]
    public class DamageableEvents {
        public UnityEvent OnDeath, OnChangeHealth, OnResetDamage, OnHitWhileInvulnerable, OnBecomeVulnerable;
    }

    public int MaxHp { get; private set; } = 100;
    public int CurrentHp { get; private set; } = 100;

    // [Range(0.0f, 360.0f)]
    // public float hitAngle = 360.0f;
    // [Range(0.0f, 360.0f)]
    // public float hitForwardRotation = 360.0f;
    public float invulnerabiltyTime;
    public DamageableEvents events; 

    public System.Action<DamageMessage> OnDamaged;

    public bool IsDead { get { return CurrentHp <= 0; }}
    public bool IsInvulnerable { get { return invulnerableTimerCR != null; }}
    public int MissingHp { get { return MaxHp - CurrentHp; }}

    float _timeLastDamaged;

    public void ResetDamage(bool removeInvulnerability) {
        if(IsDead) 
            return;
        if(removeInvulnerability && invulnerableTimerCR != null) {
            StopCoroutine(invulnerableTimerCR);
            events.OnBecomeVulnerable.Invoke();
        }
        CurrentHp = MaxHp;
        events.OnResetDamage?.Invoke();
        events.OnChangeHealth?.Invoke();
    }

    public void ApplyDamage(DamageMessage data) {
        if(IsDead) return;

        if(IsInvulnerable) {
            events.OnHitWhileInvulnerable.Invoke();
            return;
        }
        // if(!IsValidDamageDir(data))
        //     return;

        OnDamaged?.Invoke(data);
        AddToCurrentHp(-data.amount);
        data.damager.OnDamageDealt(this);
        _timeLastDamaged = Time.time;

        if(CurrentHp <= 0) {
            StartCoroutine(EndOfFrameDeath());
        }
        else if(invulnerabiltyTime > 0f)
            invulnerableTimerCR = StartCoroutine( InvulnerabilityTimer(invulnerabiltyTime) );
    }

    // bool IsValidDamageDir(DamageMessage data) {
    //     Vector3 forward = transform.forward;
    //     forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;
    //     //project the direction to damager to the plane formed by the direction of damage
    //     Vector3 damagerDirection = data.damager.Self.transform.position - transform.position;
    //     damagerDirection -= transform.up * Vector3.Dot(transform.up, damagerDirection);
    //     if(Vector3.Angle(forward, damagerDirection) > hitAngle * 0.5f) 
    //         return false;

    //     return true;
    // }

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
        if(MissingHp <= 0) {
            Debug.Log("Not missing hp");
            return false;
        }
        if(requireFullAmount && MissingHp < amount) {
            Debug.Log($"Not missing {amount} hp");
            return false;
        }
        
        AddToCurrentHp(amount);
        return true;
    }

    public void SetMaxHp(int value) {
        MaxHp = value;
    }

    public void AddToCurrentHp(int amount) {
        CurrentHp += amount;
        events.OnChangeHealth.Invoke();
    }


// #if UNITY_EDITOR
//     private void OnDrawGizmosSelected()
//     {
//         Vector3 forward = transform.forward;
//         forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

//         if (Event.current.type == EventType.Repaint)
//         {
//             UnityEditor.Handles.color = Color.blue;
//             UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(forward), 1.0f,
//                 EventType.Repaint);
//         }
//         UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
//         forward = Quaternion.AngleAxis(-hitAngle * 0.5f, transform.up) * forward;
//         UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, forward, hitAngle, 1.0f);
//     }
// #endif
}
}