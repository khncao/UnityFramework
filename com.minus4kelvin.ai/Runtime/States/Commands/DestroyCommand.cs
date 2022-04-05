using UnityEngine;

namespace m4k.AI {
public struct Destroy : IState, ITargetHandler {
    public int priority { get; set; }
    public StateProcessor processor { get; private set; }
    public Transform target { get; set; }
    GameObject go;

    public Destroy(GameObject go, int priority = -1, StateProcessor processor = null) {
        this.go = go;
        this.processor = processor;
        this.priority = priority;
        this.target = null;
    }

    public void OnEnter(StateProcessor processor) {
        if(go)
            MonoBehaviour.Destroy(go);
        else if(target)
            MonoBehaviour.Destroy(target.gameObject);
        else
            Debug.LogWarning("Destroyed nothing");
    }

    public bool OnUpdate() => true;
    public void OnExit() {}
}

[System.Serializable]
public class DestroyWrapper : StateWrapper {
    public GameObject go;
    
    public override IState GetState() {
        return new Destroy(go, priority);
    }
}

[CreateAssetMenu(fileName = "DestroyCommand", menuName = "Data/AI/States/DestroyCommand", order = 0)]
public class DestroyCommand : StateWrapperBase {
    [Header("Target->WrappingState(detector)")]
    public GameObject go;
    
    public override IState GetState() {
        return new Destroy(go, priority);
    }
}
}