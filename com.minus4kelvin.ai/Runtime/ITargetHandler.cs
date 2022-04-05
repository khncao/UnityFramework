
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// For passing target reference between IStates; useful for maintaining scope in nested states
/// </summary>
public interface ITargetHandler {
    Transform target { get; set; }
}
}