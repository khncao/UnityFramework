using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace m4k.AI {
public struct Flee : IState, ITargetHandler {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }
    public Transform target { get; set; }

    float _navSampleRadius;
    int _areaMask;

    Path _pathCommand;
    float _triggerSqrDistance;
    float _fleeDistance;
    float _fleeSpeed;

    public Flee(Transform target, float triggerSqrDistance, float fleeDistance, float fleeSpeed, int priority = -1, StateProcessor processor = null) {
        this.processor = processor;
        this.priority = priority;
        this.target = target;
        this._triggerSqrDistance = triggerSqrDistance;
        this._fleeDistance = fleeDistance;
        this._fleeSpeed = fleeSpeed;
        _pathCommand = new Path(this.target);
        currentCommand = null;
        _navSampleRadius = 1f;
        _areaMask = NavMesh.AllAreas;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        if(processor.TryGetComponent<NavMeshAgent>(out var agent)) {
            _navSampleRadius = agent.height * 2f;
            _areaMask = agent.areaMask;
        }
        else {
            Debug.LogWarning("No NavMeshAgent found on processor");
        }

        if(target == null) {
            Debug.LogWarning("No target in Flee state");
        }
    }

    public bool OnUpdate() {
        Vector3 direction = processor.transform.position - target.position;
        int roundedSqrDistance = (int)(direction.sqrMagnitude);

        if(currentCommand != null && _pathCommand.OnUpdate()) {
            _pathCommand.OnExit();
            currentCommand = null;
            processor.movable.Speed = -1f;
        }
        else if(currentCommand == null
        && roundedSqrDistance < _triggerSqrDistance) {
            Vector3 newPosition = processor.transform.position + (direction.normalized * _fleeDistance);

            if(NavMesh.SamplePosition(newPosition, out NavMeshHit hit, _navSampleRadius, _areaMask)) {
                newPosition = hit.position;
                
                _pathCommand.AssignTarget(newPosition);
                _pathCommand.OnEnter(processor);
                processor.movable.Speed = _fleeSpeed;

                currentCommand = _pathCommand;
            }
        }
        return false;
    }

    public void OnExit() {
        _pathCommand.OnExit();
    }
}

[System.Serializable]
public class FleeWrapper : StateWrapper {
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag")]
    public Transform target;
    public float triggerSqrDistance;
    public float fleeDistance;
    public float fleeSpeed;

    public override IState GetState() {
        return new Flee(target, triggerSqrDistance, fleeDistance, fleeSpeed, priority);
    }
}

[CreateAssetMenu(fileName = "FleeState", menuName = "Data/AI/States/FleeState", order = 0)]
public class FleeState : StateWrapperBase {
    [Header("Target->FindTargetByTag->WrappingState(detector)")]
    [Tooltip("Leaving null will try to find targetTag")]
    public Transform target;
    public float triggerSqrDistance;
    public float fleeDistance;
    public float fleeSpeed;

    public override IState GetState() {
        return new Flee(target, triggerSqrDistance, fleeDistance, fleeSpeed, priority);
    }
}
}