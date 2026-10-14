using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class HeadExplosion : MonoBehaviour
{
    [Header("References")]
    public GameObject robotHead;
    public Rigidbody headRigidbody;
    public NavMeshAgent robotAgent;

    [Header("Fire Prefab Settings")]
    public GameObject firePrefab;
    public Transform fireSpawnPoint;

    [Header("Explosion Settings")]
    public float explosionForce = 5f;

    [Header("Trainer Input")]
    [Tooltip("Drag your HeadExplosion input action here")]
    public InputActionReference fireAction;

    // --- NEW: Safety lock to prevent double-explosions ---
    private bool hasExploded = false;

    private void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable();
            fireAction.action.performed += TriggerFire;
        }
    }

    private void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.performed -= TriggerFire;
            fireAction.action.Disable();
        }
    }

    private void TriggerFire(InputAction.CallbackContext context)
    {
        // --- NEW: Stop the function if the head already exploded ---
        if (hasExploded) return;

        hasExploded = true; // Lock it down

        // 1. Stop the robot instantly
        if (robotAgent != null)
        {
            robotAgent.isStopped = true;
            robotAgent.velocity = Vector3.zero;
        }

        // 2. Unparent the head so it detaches
        if (robotHead != null)
        {
            robotHead.transform.SetParent(null);
        }

        // 3. Enable gravity and pop it off
        if (headRigidbody != null)
        {
            headRigidbody.isKinematic = false;
            headRigidbody.AddForce(Vector3.up * explosionForce, ForceMode.Impulse);
            headRigidbody.AddTorque(new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), ForceMode.Impulse);
        }

        // 4. Spawn the fire prefab at the neck
        if (firePrefab != null && fireSpawnPoint != null)
        {
            Instantiate(firePrefab, fireSpawnPoint.position, fireSpawnPoint.rotation, fireSpawnPoint);
        }
    }
}