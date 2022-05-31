
using UnityEngine;

namespace m4k.Damage {
public class ProjectileCollision : Projectile {
	public virtual void OnCollisionEnter(Collision other) {
        freeFlag = true;
        
        if(!canHurtOwner || other.transform == owner)
            return;
        if(!other.collider.TryGetComponent<IDamageable>(out var damageable)) 
            return;

        var contact = other.GetContact(0);
        var rb = other.rigidbody;

        damageable.ApplyDamage(-damage);
        damageable.ApplyForce(force, contact.point, transform.position - contact.point);
	}
}
}