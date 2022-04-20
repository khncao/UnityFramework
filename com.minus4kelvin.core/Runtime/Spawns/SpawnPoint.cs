using UnityEngine;

namespace m4k {
/// <summary>
/// Culling group cullable spawn point
/// </summary>
public class SpawnPoint : MonoBehaviour, ICullingGroupable {
    public const string CullingGroupName = "Spawnpoints";

    public float boundingSphereRadius = 1f;
    [Header("Optional trigger check if assigned")]
    public Collider col;

    BoundingSphere bs;
    bool registeredAsSpawn;

    public int cullingGroupIndex { get; set; } // this obj index in cullingGroupWrapper
    public int DistanceBandIndex { get; private set; }
    public bool Colliding { get; private set; }
    public bool Visible { get; private set; }

    private void Start() {
        if(col) {
            if(!TryGetComponent<Rigidbody>(out var rb))
                Debug.LogWarning("No rigidbody on spawnpoint with collider");
            if(!col.isTrigger)
                Debug.LogWarning("Collider on spawnpoint not trigger");
        }
        bs = new BoundingSphere(transform.position, boundingSphereRadius);

        CullingGroupManager.I.RegisterCullTarget(CullingGroupName, this, bs);
    }

    private void OnDestroy() {
        CullingGroupManager.I?.UnregisterCullTarget(CullingGroupName, cullingGroupIndex);
        SpawnManager.I?.UnregisterValidSpawnPoint(this);
    }

    private void OnTriggerEnter(Collider other) {
        Colliding = true;
        if(registeredAsSpawn) {
            SpawnManager.I?.UnregisterValidSpawnPoint(this);
            registeredAsSpawn = false;
        }
    }
    private void OnTriggerExit(Collider other) {
        Colliding = false;
        if(!registeredAsSpawn && !Visible) {
            SpawnManager.I?.RegisterValidSpawnPoint(this);
            registeredAsSpawn = true;
        }
    }

    public void OnCullingGroupStateChange(CullingGroupEvent ev) {
        DistanceBandIndex = ev.currentDistance;
        Visible = ev.isVisible;
        
        if(!registeredAsSpawn && ev.hasBecomeInvisible && !Colliding) {
            SpawnManager.I?.RegisterValidSpawnPoint(this);
            registeredAsSpawn = true;
        }
        else if(registeredAsSpawn && (ev.hasBecomeVisible || Colliding)) {
            SpawnManager.I?.UnregisterValidSpawnPoint(this);
            registeredAsSpawn = false;
        }
        // switch(ev.currentDistance) {
        //     case 0:
        //         break;
        //     case 1:
        //         break;
        //     case 2:
        //         break;
        // }
    }
}}