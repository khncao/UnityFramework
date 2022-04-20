using UnityEngine;

namespace m4k {
public class CullingGroupWrapper {
    BoundingSphere[] boundingSpheres;
    ICullingGroupable[] groupables;
    float[] boundingDistances;
    int tailIndex = 0;
    bool disposed = false;

    public float lastUpdatePositionTime { get; set; }

    public float UpdatePositionInterval { get; private set;}
    public CullingGroup Group { get; }
    public int Size { get; private set; }


    public CullingGroupWrapper(int amountBoundingSpheres, float[] boundingDistances, float updatePositionInterval) {
        Size = amountBoundingSpheres;
        this.UpdatePositionInterval = updatePositionInterval;
        this.boundingDistances = boundingDistances;
        Group = new CullingGroup();
        boundingSpheres = new BoundingSphere[amountBoundingSpheres];
        groupables = new ICullingGroupable[amountBoundingSpheres];

        Group.SetBoundingSpheres(boundingSpheres);
        Group.SetBoundingDistances(boundingDistances);
        Group.onStateChanged = OnCullingGroupStateChange;
    }

    public int AddBoundingSphere(ICullingGroupable groupable, Renderer rend) {
        return AddBoundingSphere(groupable, rend.bounds.center, Mathf.Max(rend.bounds.extents.x, Mathf.Max(rend.bounds.extents.y, rend.bounds.extents.z)));
    }
    public int AddBoundingSphere(ICullingGroupable groupable, Vector3 center, float radius) {
        return AddBoundingSphere(groupable, new BoundingSphere(center, radius));
    }
    public int AddBoundingSphere(ICullingGroupable groupable, BoundingSphere bs) {
        if(tailIndex == boundingSpheres.Length) {
            Debug.LogWarning("Insufficient array size");
            return -1;
        }
        int index = tailIndex;
        boundingSpheres[index] = bs;
        groupables[index] = groupable;

        tailIndex++;
        Group.SetBoundingSphereCount(tailIndex);

        return index;
    }

    public void RemoveBoundingSphere(int index) {
        if(disposed || Group == null || groupables == null || boundingSpheres == null) 
            return;

        if(tailIndex > 1) {
            groupables[tailIndex - 1].cullingGroupIndex = index;
            Group.EraseSwapBack(index);
        }
        
        Group.SetBoundingSphereCount(tailIndex);
        tailIndex--;
    }

    public void SetCam(Camera cam) {
        Group.targetCamera = cam;
    }

    public void SetReference(Transform transform) {
        Group.SetDistanceReferencePoint(transform);
    }

    public void UpdatePositions() {
        for(int i = 0; i < tailIndex; ++i) {
            boundingSpheres[i].position = groupables[i].transform.position;
        }
    }

    public int GetDistanceBandIndex(float distance) {
        if(distance < 0f) Debug.LogError("Distance can't be less than 0");

        int bandIndex = System.Array.FindIndex(boundingDistances, x=>x >= distance);
        if(bandIndex == -1) bandIndex = boundingDistances.Length - 1;
        return bandIndex;
    }

    public int QueryIndices(int[] result, bool isVisible, int startIndex = 0) {
        return Group.QueryIndices(isVisible, result, startIndex);
    }

    public int QueryIndices(int[] result, int distanceBandIndex, int startIndex = 0) {
        return Group.QueryIndices(distanceBandIndex, result, startIndex);
    }

    public int QueryIndices(int[] result, bool isVisible, int distanceBandIndex, int startIndex = 0) {
        return Group.QueryIndices(isVisible, distanceBandIndex, result, startIndex);
    }

    public void Cleanup() {
        Group.Dispose();
        disposed = true;
    }

    void OnCullingGroupStateChange(CullingGroupEvent ev) {
        groupables[ev.index]?.OnCullingGroupStateChange(ev);
    }
}
}