using UnityEngine;

// PC-PLA-P7 – when the player touches it, this becomes his new respawn point.
// Put it on the Checkpoint object (or its CheckpointTrigger child).
// It needs a Box Collider with "Is Trigger" checked.
public class Checkpoint : MonoBehaviour
{
    Transform respawnPoint;
    bool activated = false;

    void Start()
    {
        // make sure the collider is a trigger
        Collider zone = GetComponent<Collider>();
        if (zone == null) Debug.LogWarning("Checkpoint: add a Box Collider (Is Trigger) to " + name);
        else zone.isTrigger = true;

        // find "RespawnPoint" in the Checkpoint prefab
        Transform checkpointRoot = transform;
        if (transform.parent != null) checkpointRoot = transform.parent;
        respawnPoint = checkpointRoot.Find("RespawnPoint");
        if (respawnPoint == null) respawnPoint = transform;

        Debug.Log("Checkpoint: ready on " + name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (activated) return; // only once

        // is it the player? (the player is the object that has PlayerPhysics)
        PlayerPhysics player = other.GetComponentInParent<PlayerPhysics>();
        if (player == null) return;

        activated = true;
        // a little above the floor so the player does not appear inside it
        player.SetCheckpoint(respawnPoint.position + Vector3.up * 0.5f);
        Debug.Log("Checkpoint: activated, new respawn point = " + respawnPoint.position);

        // turn the arch green
        CheckpointFX fx = GetComponentInParent<CheckpointFX>();
        if (fx != null)
        {
            fx.Activate();
        }
    }
}