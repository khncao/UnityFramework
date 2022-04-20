
using UnityEngine;
using UnityEngine.Animations;

/// <summary>
/// Parent or constrain object to bone to move object with animations. Useful for cloth colliders, non rigged equipment(helmets, held items)
/// </summary>
public class ParentToBone : MonoBehaviour {
    [Header("Top-down priority")]
    public HumanBodyBones humanBone;
    public bool useHumanBone;
    public Transform boneTarget;
    public string boneName;

    public Transform GetTargetBoneTransform(Animator targetAnim) {
        Transform targetBone = null;
        if(useHumanBone)
            targetBone = targetAnim.GetBoneTransform(humanBone);

        if(boneTarget) {
            boneName = boneTarget.name;
        }
        if(string.IsNullOrEmpty(boneName)) {
            Debug.LogError("No bone name to find");
        }
        
        targetBone = targetAnim.transform.Find(boneName);
        if(targetBone == null) {
            Debug.LogError("Target bone not found");
        }

        return targetBone;
    }

    public void SetupParent(Animator targetAnim, bool keepWorldPos = true) {
        transform.SetParent(GetTargetBoneTransform(targetAnim), keepWorldPos);
    }

    /// <summary>
    /// Find or create simple parent constraint
    /// </summary>
    /// <param name="targetAnim"></param>
    /// <param name="keepWorldPos"></param>
    public void SetupConstraint(Animator targetAnim, bool keepWorldPos = true) {
        var targetBone = GetTargetBoneTransform(targetAnim);
        if(!targetBone) return;
        if(!keepWorldPos) {
            transform.position = targetBone.position;
            transform.rotation = targetBone.rotation;
        }
        if(!TryGetComponent<ParentConstraint>(out var constraint)) {
            constraint = gameObject.AddComponent<ParentConstraint>();
        }
        constraint.AddSource(new ConstraintSource() {
            sourceTransform = targetBone,
            weight = 1.0f
        });
        constraint.constraintActive = true;
        constraint.weight = 1f;

    }
}