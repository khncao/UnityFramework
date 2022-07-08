

using UnityEngine;

namespace m4k.TimedInput {
public class TimedNoteProjectile : MonoBehaviour {
    // gameObject pool

    public void Initialize() {

    }

    // public void OnFixedUpdate() {} // managed movement

    private void OnTriggerEnter(Collider other) {
        // set proper layer
        // player controlled collider
        // kill zone collider
        // free to pool
    }
}
}