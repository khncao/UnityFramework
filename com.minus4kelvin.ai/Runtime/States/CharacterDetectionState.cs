using UnityEngine;
using m4k.Characters;

namespace m4k.AI {
public class CharacterDetection : IState {
    [System.Serializable]
    public struct Data {
        public GameObjectListSO targetList;
        public float maxSquaredRange;

        [Range(0f, 360f)]
        public float viewAngleOverride;
        
        [SerializeField]
        [InspectInline(canCreateSubasset = true)]
        private StateWrapperBase detectingStateWrapper, gotoStateWrapper;

        public IState detectingState;
        public IState gotoState;

        public bool Init() {
            if(detectingStateWrapper == null
            || gotoStateWrapper == null) 
                return false;
            detectingState = detectingStateWrapper.GetState();
            gotoState = gotoStateWrapper.GetState();
            return true;
        }
    }

    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }

    Data data;
    DetectRadiusAngleGameObject _detector;
    float _lastCheckTime;

    public CharacterDetection(Data data, int priority) {
        this.data = data;
        if(!this.data.Init()) {
            Debug.LogError("Null state");
        }
        this._lastCheckTime = 0f;
        this.priority = priority;
        this._detector = new DetectRadiusAngleGameObject(null, data.targetList.GetList(), data.maxSquaredRange, data.viewAngleOverride);
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;
        data.detectingState.OnEnter(processor);

        if(_detector.self == null)
            _detector.self = processor.gameObject;
    }

    public bool OnUpdate() {
        if((Time.time - _lastCheckTime) > processor.detectionCooldown && 
        _detector.UpdateHits()) 
        {
            _lastCheckTime = Time.time;
            
            if(data.gotoState is ITargetHandler handler) {
                handler.target = _detector.GetCachedClosest().transform;
            }
            processor.TryChangeState(data.gotoState, true);
            return false; // prevent onStateComplete call
        }
        return data.detectingState.OnUpdate();
    }

    public void OnExit() {
        data.detectingState.OnExit();
    }
}

[CreateAssetMenu(fileName = "CharacterDetectionState", menuName = "Data/AI/States/CharacterDetectionState", order = 0)]
public class CharacterDetectionState : StateWrapperBase {
    public CharacterDetection.Data data;

    public override IState GetState() {
        return new CharacterDetection(data, priority);
    }
}
}