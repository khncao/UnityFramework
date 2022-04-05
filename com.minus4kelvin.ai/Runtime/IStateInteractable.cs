using UnityEngine;

namespace m4k.AI {
public interface IStateInteractable {
    Transform transform { get; }
    void OnStateInteract(IState state);
    bool CanStateInteract(IState state);
}
}