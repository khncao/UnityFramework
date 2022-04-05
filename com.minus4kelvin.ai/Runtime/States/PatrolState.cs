using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Ordered or random loop super state for sequentially patrolling set of transform points
/// </summary>
public class Patrol : IState {
    public enum PatrolType {
        Loop, Random, PingPong, 
    }
    public int priority { get; private set; }
    public StateProcessor processor { get; private set; }
    public IState currentCommand { get; private set; }

    List<Transform> _orderedPoints;
    Path _pathCommand;
    IState _betweenPatrolState;
    PatrolType _patrolType;
    string _pointsTag;

    int _currentIndex;
    bool _reverseFlag;
    bool _pathFlag;

    public Patrol(List<Transform> orderedPoints, string pointsTag, PatrolType patrolType, IState betweenPatrolState, int priority = -1) {
        this._orderedPoints = orderedPoints;
        this._patrolType = patrolType;
        this._pointsTag = pointsTag;
        this._betweenPatrolState = betweenPatrolState;
        this.priority = priority;
    }

    public void OnEnter(StateProcessor processor) {
        this.processor = processor;

        if(_orderedPoints == null || _orderedPoints.Count < 1 && !string.IsNullOrEmpty(_pointsTag)) {
            if(SceneController.TryGetSceneController(processor.gameObject.scene.name, out var sceneController))
                _orderedPoints = sceneController.GetKeyPoints(_pointsTag);

        }
        if(_orderedPoints == null) {
            Debug.LogError("Invalid ordered points in patrol state");
            return;
        }
        // get closest point
        Transform closest = null;
        processor.transform.GetClosest<Transform>(_orderedPoints, out closest);
        _currentIndex = _orderedPoints.FindIndex(x=>x == closest);

        _pathCommand = new Path(_orderedPoints[_currentIndex]);
        _pathCommand.OnEnter(processor);
        currentCommand = _pathCommand;
        _pathFlag = false;
    }

    public bool OnUpdate() {
        if(currentCommand != null && currentCommand.OnUpdate()) {
            currentCommand.OnExit();
            currentCommand = null;
        }
        else if(currentCommand == null) {
            if(_pathFlag || _betweenPatrolState == null) {
                UpdatePointIndex();

                _pathCommand.target = _orderedPoints[_currentIndex];
                currentCommand = _pathCommand;
                currentCommand.OnEnter(processor);
                _pathFlag = false;
            }
            else {
                currentCommand = _betweenPatrolState;
                _betweenPatrolState.OnEnter(processor);
                _pathFlag = true;
            }
        }
        return false;
    }

    void UpdatePointIndex() {
        switch(_patrolType) {
            case PatrolType.Loop:
                _currentIndex = (_currentIndex + 1) % _orderedPoints.Count;
                break;
            case PatrolType.Random:
                _currentIndex = Random.Range(0, _orderedPoints.Count);
                break;
            case PatrolType.PingPong:
                _currentIndex += _reverseFlag ? -1 : 1;
                if(_currentIndex == 0 || _currentIndex == _orderedPoints.Count - 1)
                    _reverseFlag = !_reverseFlag;
                break;
        }
    }

    public void OnExit() {
        currentCommand?.OnExit();
        currentCommand = null;
    }
}

[System.Serializable]
public class PatrolWrapper : StateWrapper {
    public List<Transform> points;
    public string pointsTag;
    public Patrol.PatrolType patrolType;
    [SerializeReference]
#if SERIALIZE_REFS
    [SubclassSelector]
#endif
    public StateWrapper betweenPatrolState;

    public override IState GetState() {
        return new Patrol(points, pointsTag, patrolType, betweenPatrolState?.GetState(), priority);
    }
}

[CreateAssetMenu(fileName = "PatrolState", menuName = "Data/AI/States/PatrolState", order = 0)]
public class PatrolState : StateWrapperBase {
    public List<Transform> points;
    public string pointsTag;
    public Patrol.PatrolType patrolType;

    [InspectInline(canCreateSubasset = true)]
    public StateWrapperBase betweenPatrolState;

    public override IState GetState() {
        return new Patrol(points, pointsTag, patrolType, betweenPatrolState?.GetState(), priority);
    }
}
}