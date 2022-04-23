using System.Collections.Generic;
using UnityEngine;
using m4k.Characters;

namespace m4k {
[System.Serializable]
public class CullingGroupProfile {
    public string name;
    [Tooltip("Interval less than 0 to disable updating boundingSphere positions. Larger intervals for more frequent updates")]
    public float updatePositionInterval;
    public int size;
    public float[] boundingDistances;
}

public class CullingGroupManager : Singleton<CullingGroupManager> {
    public List<CullingGroupProfile> profiles;

    Dictionary<string, CullingGroupWrapper> cullingGroups = new Dictionary<string, CullingGroupWrapper>();

    protected override void Awake() {
        base.Awake();
        if(m_ShuttingDown) 
            return;

        for(int i = 0; i < profiles.Count; ++i) {
            var groupInstance = new CullingGroupWrapper(profiles[i].size, profiles[i].boundingDistances, profiles[i].updatePositionInterval);
            groupInstance.SetCam(Camera.main);

            cullingGroups.Add(profiles[i].name, groupInstance);
        }
    }

    private void Start() {
        CharacterManager.I.onPlayerRegistered -= OnPlayerRegister;
        CharacterManager.I.onPlayerRegistered += OnPlayerRegister;
    }

    private void OnDestroy() {
        foreach(var e in cullingGroups) 
            e.Value.Cleanup();
    }

    private void Update() {
        foreach(var group in cullingGroups) {
            if(group.Value.UpdatePositionInterval < 0) 
                continue;
            if(Time.time - group.Value.lastUpdatePositionTime < group.Value.UpdatePositionInterval) 
                continue;

            group.Value.lastUpdatePositionTime = Time.time;
            group.Value.UpdatePositions();
        }
    }

    public int RegisterCullTarget(string groupName, ICullingGroupable groupable, Renderer rend) {
        if(TryGetCullingGroup(groupName, out var group)) {
            return group.AddBoundingSphere(groupable, rend);
        }
        return -1;
    }
    public int RegisterCullTarget(string groupName, ICullingGroupable groupable, BoundingSphere bs) {
        if(TryGetCullingGroup(groupName, out var group)) {
            return group.AddBoundingSphere(groupable, bs);
        }
        Debug.LogWarning($"{groupName} culling group not found");
        return -1;
    }

    public void UnregisterCullTarget(string groupName, int index) {
        if(TryGetCullingGroup(groupName, out var group)) {
            group.RemoveBoundingSphere(index);
        }
    }

    public bool TryGetCullingGroup(string groupName, out CullingGroupWrapper groupInstance) {
        return cullingGroups.TryGetValue(groupName, out groupInstance);
    }

    void OnPlayerRegister(CharacterControl player) {
        foreach(var e in cullingGroups)
            e.Value.Group.SetDistanceReferencePoint(player.transform);
    }
}}