using UnityEngine;

// Put this on PlayerArmature (the object with the CharacterController).
// Starter Assets moves the player with the keyboard; this script adds the pushes from traps.
// Traps use AddImpulse / AddForce to push the player.
// Checkpoints use SetCheckpoint, the kill zone uses Respawn.
public class PlayerPhysics : MonoBehaviour
{
    public float gravity = -20f;   // slows down an upward push (trampoline)
    public float friction = 3f;    // how fast a sideways push stops

    CharacterController controller;
    Vector3 pushVelocity;          // extra speed given by traps
    Vector3 respawnPosition;       // where we go back when we fall

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogWarning("PlayerPhysics: put this script on the player (the object with the CharacterController).");
        }
        respawnPosition = transform.position; // the start is the first checkpoint
    }

    // One push (bumper, trampoline, launcher)
    public void AddImpulse(Vector3 force)
    {
        pushVelocity += force;

        // tell the animation we were pushed (to do a flip)
        BeanAnimator animator = GetComponentInChildren<BeanAnimator>();
        if (animator != null)
        {
            animator.OnPushed(force);
        }
    }

    // A push that lasts (fan): call it every frame while inside the wind
    public void AddForce(Vector3 force)
    {
        pushVelocity += force * Time.deltaTime;
    }

    public void SetCheckpoint(Vector3 position)
    {
        respawnPosition = position;
    }

    public void Respawn()
    {
        Debug.Log("PlayerPhysics: respawn at " + respawnPosition);
        pushVelocity = Vector3.zero;

        // the CharacterController must be off to teleport the player
        if (controller != null) controller.enabled = false;
        transform.position = respawnPosition;
        if (controller != null) controller.enabled = true;
    }

    void Update()
    {
        if (controller == null) return;

        // no push from a trap: do nothing, Starter Assets moves the player normally
        if (pushVelocity.magnitude < 0.1f)
        {
            pushVelocity = Vector3.zero;
            return;
        }

        // move the player with the push (added to the normal movement)
        controller.Move(pushVelocity * Time.deltaTime);

        // the sideways push slows down little by little
        pushVelocity.x -= pushVelocity.x * friction * Time.deltaTime;
        pushVelocity.z -= pushVelocity.z * friction * Time.deltaTime;

        // the upward push slows down because of gravity
        // (it never goes below 0: falling is already done by Starter Assets)
        pushVelocity.y += gravity * Time.deltaTime;
        if (pushVelocity.y < 0)
        {
            pushVelocity.y = 0;
        }
    }
}