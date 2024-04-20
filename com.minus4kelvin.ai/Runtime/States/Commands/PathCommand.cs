using UnityEngine;

namespace m4k.AI
{
    public struct Path : IState, ITargetHandler
    {
        public int priority { get; set; }
        public StateProcessor processor { get; private set; }
        public Transform target { get; set; }

        Vector3 _targetPosition;
        float _speedMult;

        public Path(Transform t = null, float speedMult = 1f, Vector3 targetPos = default, int priority = -1, StateProcessor processor = null)
        {
            target = t;
            _targetPosition = targetPos;
            _speedMult = speedMult;
            this.processor = processor;
            this.priority = priority;
        }

        public Path(Vector3 pos, int priority = -1) : this(null, 1f, pos) { }

        public Path(float speedMult, int priority = -1) : this(null, speedMult) { }

        public void OnEnter(StateProcessor processor)
        {
            this.processor = processor;

            if (target)
            {
                processor.movable.SetTarget(target);
                processor.movable.Speed *= _speedMult;
            }
            else
                processor.movable.SetTarget(_targetPosition);

            processor.ToggleProximityTrigger(true);
            // processor.movable.OnArrive += OnArrive;
            // processor.onArrive += OnArrive;
        }

        public bool OnUpdate()
        {
            return !processor.movable.IsMoving;
        }

        public void OnExit()
        {
            // processor.movable.OnArrive -= OnArrive;
            // processor.onArrive -= OnArrive;
            processor.movable.Stop();
            processor.movable.Speed = -1f;
        }

        public void AssignTarget(Vector3 pos)
        {
            _targetPosition = pos;
        }

        // void OnArrive() {
        //     // processor.movable.Stop();
        //     this._arrived = true;
        // }
    }

    [System.Serializable]
    public class PathWrapper : StateWrapper
    {
        [Header("Target->Position->WrappingState(detector)")]
        [Tooltip("Will fallback to targetPosition if null")]
        public Transform targetTransform;
        public Vector3 targetPosition;

        public override IState GetState()
        {
            if (targetTransform)
                return new Path(targetTransform, priority);
            else
                return new Path(targetPosition, priority);
        }
    }

    [CreateAssetMenu(fileName = "PathCommand", menuName = "Data/AI/States/PathCommand", order = 0)]
    public class PathCommand : StateWrapperBase
    {
        public float speedMult = 2f;

        public override IState GetState()
        {
            return new Path(speedMult, priority);
        }
    }
}