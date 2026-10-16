using UnityEngine;

// PC-PLA-P4 – big invisible trigger under the level.
// When the player falls into it, he goes back to the last checkpoint.
// Put it on the KillZone object (it needs a Box Collider with "Is Trigger" checked).
public class KillZone : MonoBehaviour
{
    void Start()
    {
        // make sure the collider is a trigger
        Collider zone = GetComponent<Collider>();
        if (zone == null) Debug.LogWarning("KillZone: add a Box Collider to " + name);
        else zone.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // is it the player? (the player is the object that has PlayerPhysics)
        PlayerPhysics player = other.GetComponentInParent<PlayerPhysics>();
        if (player == null) return;

        Debug.Log("KillZone: the player fell -> respawn");
        player.Respawn();
    }
}