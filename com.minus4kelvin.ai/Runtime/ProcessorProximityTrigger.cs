using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Placed on navigation target object to signal target is within radius. May phase out in favor of IMoveTargetable GetTargetDistance or OnTargetDistance
/// </summary>
public class ProcessorProximityTrigger : MonoBehaviour
{
    public StateProcessor processor;
    public Collider processorCol;

    private void OnTriggerEnter(Collider other) {
        if(other == processorCol) {
            processor.OnArrive();
        }
    }
}
}