using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k {
public class DetectRadiusAngleTransform : DetectRadiusAngle<Transform> {
        public DetectRadiusAngleTransform(Transform self, IList<Transform> others, float maxSquaredRange, float viewAngles = 0, Predicate<Transform> query = null) : base(self, others, maxSquaredRange, viewAngles, query)
        {
        }

        protected override bool IsValid(Transform other) {
        Transform otherTransform = other;
        if(!detectSelf && otherTransform == self)
            return false;

        Vector3 direction = otherTransform.position - self.position;
        float sqrMagnitude = direction.sqrMagnitude;

        if(sqrMagnitude > _maxSquaredRange)
            return false;

        if(_viewAngles != 0f) {
            // direction -= self.up * Vector3.Dot(self.up, direction);
            direction.y = 0f;

            if(Vector3.Angle(self.forward, direction) > _viewAngles * 0.5f) 
                return false;

            // var dir = self.InverseTransformDirection(otherDirection);
            // var angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            // if(Mathf.Abs(angle) > _viewAngles * 0.5f)
            //     return false;
        }

        if(sqrMagnitude < _closestDistance) {
            _closestDistance = sqrMagnitude;
            _closest = other;
        }

        return true;
    }
}
}