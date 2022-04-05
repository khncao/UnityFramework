
using UnityEngine;

namespace m4k.AI {
public class SphereDetection : IState {
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    
    IState _detectingState;
    IState _gotoState;
    float _lastCheckTime;
    float _maxSquaredRange;
    Transform _self;
    LayerMask _layers;
    string _tag;
    DetectRadiusAngle<Collider> _detector;
    Collider[] _hits;

    public SphereDetection(IState detectingState, IState gotoState, Transform self, LayerMask layers, string tag, float maxSquaredRange, float viewAngles = 0f, int priority = -1, StateProcessor processor = null) {
        this._detectingState = detectingState;
        this._gotoState = gotoState;
        this.priority = priority;
        this.processor = processor;
        this._lastCheckTime = 0f;
        this._maxSquaredRange = maxSquaredRange;
        this._layers = layers;
        this._tag = tag;
        this._self = self;

        this._hits = new Collider[32];
        
        System.Predicate<Collider> predicate = null;
        if(!string.IsNullOrEmpty(_tag)) {
            predicate = x => x.CompareTag(_tag);
        }
        _detector = new DetectRadiusAngle<Collider>(self, _hits, maxSquaredRange, viewAngles, predicate);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        _detectingState.OnEnter(processor);

        if(_detector.self == null) {
            _detector.self = processor.transform;
            _self = processor.transform;
        }
    }

    public bool OnUpdate() {
        if((Time.time - _lastCheckTime) < processor.detectionCooldown) {
            return false; // return if check on cooldown
        }
        _lastCheckTime = Time.time;

        // TODO: move to utility class; variable grading steps; split over frames
        if(Physics.OverlapSphereNonAlloc(_self.position, _maxSquaredRange / 2f, _hits, _layers, QueryTriggerInteraction.Collide) < 1) {
            if(Physics.OverlapSphereNonAlloc(_self.position, _maxSquaredRange, _hits, _layers, QueryTriggerInteraction.Collide) < 1)
                return false; // return if no collider hits
        }
        
        if(_detector.UpdateHits()) 
        {
            if(_gotoState is ITargetHandler handler) {
                handler.target = _detector.GetCachedClosest().transform;
            }
            processor.TryChangeState(_gotoState, true);
            return false; // prevent onStateComplete call
        }
        return _detectingState.OnUpdate();
    }

    public void OnExit() {
        _detectingState.OnExit();
    }
}

[System.Serializable]
public class SphereDetectionStateWrapper : StateWrapper {
    public LayerMask layerMask;
    public string tag;
    public float maxSquaredRange;
    [Range(0f, 360f)]
    public float viewAngleOverride;

    [Tooltip("Basis to perform detection. Will use processor transform if null")]
    public Transform self;
    
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public StateWrapper detectingState, gotoState;


    public override IState GetState() {
        return new SphereDetection(detectingState.GetState(), gotoState.GetState(), self, layerMask, tag, maxSquaredRange, viewAngleOverride, priority);
    }
}

[CreateAssetMenu(fileName = "SphereDetectionState", menuName = "Data/AI/States/SphereDetectionState", order = 0)]
public class SphereDetectionState : StateWrapperBase {
    public LayerMask layerMask;
    public string tag;
    public float maxSquaredRange;
    [Range(0f, 360f)]
    public float viewAngleOverride;

    [Tooltip("Basis to perform detection. Will use processor transform if null")]
    public Transform self;
    
    [InspectInline(canCreateSubasset = true)]
    public StateWrapperBase detectingState, gotoState;

    public override IState GetState() {
        return new SphereDetection(detectingState.GetState(), gotoState.GetState(), self, layerMask, tag, maxSquaredRange, viewAngleOverride, priority);
    }
}
}