using System;
using System.Collections.Generic;
using UnityEngine;

namespace m4k.AI {
/// <summary>
/// Base for deriving state wrappers used in StatesProfile and other editor editing applications. Requires SerializeReference attribute and custom editor
/// </summary>
[Serializable]
public abstract class StateWrapper {
    [Range(-100, 100)]
    public int priority;
    public Conditions conditions;

    public abstract IState GetState();
}

/// <summary>
/// ScriptableObject IState base wrapper
/// </summary>
public abstract class StateWrapperBase : ScriptableObject {
    public int priority;
    public Conditions conditions;

    public abstract IState GetState();
}
}