using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m4k.AI {
// TODO: IState/state wrapper registry/interface and GetState integration for custom script states
[CreateAssetMenu(fileName = "StatesProfile", menuName = "Data/AI/States Profile", order = 0)]
public class StatesProfile : ScriptableObject {
    const int SubMinPriority = -101;

    [System.Serializable]
    public struct NotedWrapper {
        public string description;
        [InspectInline]
        public StateWrapperBase stateWrapperBase;
    }
    [System.Serializable]
    public struct EventState {
        public string description;
        public bool forceChange;
        public bool queueIfBusy;
        public UnityEventSO eventSO;
        [InspectInline]
        public StateWrapperBase stateWrapperBase;
    }

    [Header("Get highest priority state with conditions met\nCompetes with top state of processor state queue")]
    [InspectInline]
    public List<StateWrapperBase> stateWrapperBases;
    [InspectInline]
    public StateWrapperBase defaultStateWrapperBase;

    [Header("States handled on event invoke")]
    public List<EventState> persistentEventStateListeners;

    [Header("Tag, track, maintain references; for nested editing\n Use context menu to populate all subassets of this asset")]
    public List<NotedWrapper> sketchboard;

    Dictionary<StateProcessor, Dictionary<StateWrapperBase, IState>> processorStateCache = new Dictionary<StateProcessor, Dictionary<StateWrapperBase, IState>>();

    private void Awake() {
        Reset();
    }

    public void Reset() {
        processorStateCache = new Dictionary<StateProcessor, Dictionary<StateWrapperBase, IState>>();
    }

    public IState GetState(StateProcessor processor) {
        int priorityThreshold = processor != null && processor.currentState != null ? processor.currentState.priority : SubMinPriority;
        IState state = null;

        for(int i = 0; i < stateWrapperBases.Count; ++i) 
        {
            if(stateWrapperBases[i].priority > priorityThreshold) {
                if(!stateWrapperBases[i].conditions.CheckCompleteReqs())
                    continue;

                state = GetCachedOrNewState(processor, stateWrapperBases[i], priorityThreshold);
                priorityThreshold = state.priority;
            }
        }
        if(state == null) {
            state = GetCachedOrNewState(processor, defaultStateWrapperBase, SubMinPriority);
        }

        return state;
    }

    IState GetCachedOrNewState(StateProcessor processor, StateWrapperBase stateWrapper, int priorityThreshold) {
        if(processor == null || stateWrapper == null) {
            return null;
        }

        if(stateWrapper.priority > priorityThreshold) {
            IState state = null;

            if(!processorStateCache.TryGetValue(processor, out var cache)) {
                cache = new Dictionary<StateWrapperBase, IState>();
                processorStateCache.Add(processor, cache);
            }
            if(!cache.TryGetValue(stateWrapper, out state)) {
                state = stateWrapper.GetState();
                cache.Add(stateWrapper, state);
            }
                
            return state;
        }

        Debug.LogWarning("No valid state returned");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Validate StateWrapperBases")]
    void ValidateStates() {
        Dictionary<int, int> priorityCountDict = new Dictionary<int, int>();

        foreach(var s in stateWrapperBases) {
            if(!s) continue;
            if(priorityCountDict.ContainsKey(s.priority)){
                priorityCountDict[s.priority]++;
            }
            else
                priorityCountDict.Add(s.priority, 1);
        }
        foreach(var e in priorityCountDict) {
            if(e.Value > 1) {
                Debug.Log($"{e.Value} states have {e.Key} priority. If the states do not have conditions, the earliest element will always be returned");
            }
        }
    }

    [ContextMenu("Add Subassets to Sketchboard")]
    void AddSubassetsToSketchboard() {
        var assetPath = AssetDatabase.GetAssetPath(this);
        var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);

        foreach(var i in subAssets) {
            if(i is StateWrapperBase state && !sketchboard.Exists(x=>x.stateWrapperBase == state))
                sketchboard.Add(new NotedWrapper {
                    description = state.name,
                    stateWrapperBase = state
                });
        }
        AssetDatabase.ImportAsset(assetPath);
    }

    [ContextMenu("Match Sketchboard Subasset Names To Note")]
    void MatchSubassetNamesToNote() {
        var assetPath = AssetDatabase.GetAssetPath(this);

        foreach(var i in sketchboard) {
            if(!i.stateWrapperBase || string.IsNullOrEmpty(i.description))
                continue;
            i.stateWrapperBase.name = i.description;
        }
        AssetDatabase.ImportAsset(assetPath);
    }

    [ContextMenu("Reset Sketchboard Subasset Names To TypeName")]
    void ResetSubassetNamesToDefault() {
        var assetPath = AssetDatabase.GetAssetPath(this);

        foreach(var i in sketchboard) {
            if(!i.stateWrapperBase)
                continue;
            var s = i.stateWrapperBase.GetType().ToString().Split('.');
            i.stateWrapperBase.name = s[s.Length - 1];
        }
        AssetDatabase.ImportAsset(assetPath);
    }
#endif
}

}