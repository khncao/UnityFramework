using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEvents : MonoBehaviour
{
    [Header("Audio")]
    public RandomAudioPlayer landingAudio;
    public RandomAudioPlayer footstepAudio;

    private void OnFootstep(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            footstepAudio.PlayRandomClip();
        }
    }

    private void OnLand(AnimationEvent animationEvent) {
        if (animationEvent.animatorClipInfo.weight > 0.5f) {
            footstepAudio.PlayRandomClip();
        }
    }
}
