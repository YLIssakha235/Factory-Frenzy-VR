using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FireGrenadeManager : MonoBehaviour
{
    [Header("Grenade Settings")]
    [Tooltip("Radius of the extinguishing explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [Tooltip("Damage dealt to fires (should be high enough to instantly kill it)")]
    [SerializeField] private float extinguishingPower = 100f;

    // ===== ADDED: foam burst settings =====
    [Header("Foam Burst")]
    [Tooltip("Prefab with a white Particle System (foam) played at the explosion")]
    [SerializeField] private GameObject foamBurstPrefab;
    [Tooltip("Safety: destroy the foam object after this time (seconds)")]
    [SerializeField] private float foamLifetime = 3f;
    // ======================================

    // Variables from whiteboard
    private bool grenadeActivated = false;
    private bool onCollision = false;
    private float timer = 2.5f;

    private XRGrabInteractable xrGrab;

    void Start()
    {
        xrGrab = GetComponent<XRGrabInteractable>();

        // Link the XR events to the whiteboard methods
        xrGrab.activated.AddListener(Activated);
        xrGrab.selectExited.AddListener(OnGrenadeRelease);
    }

    // Triggered when pulling the VR trigger while holding the sphere
    private void Activated(ActivateEventArgs arg)
    {
        grenadeActivated = true;
        Debug.Log("Grenade Activated! Ready to throw.");
    }

    // Triggered when you let go of the sphere
    private void OnGrenadeRelease(SelectExitEventArgs arg)
    {
        // If activated before throwing, start the 2.5 second fuse timer
        if (grenadeActivated)
        {
            StartCoroutine(ExplosionTimer());
        }
    }

    private IEnumerator ExplosionTimer()
    {
        yield return new WaitForSeconds(timer);
        Explode();
    }

    // Triggered when the sphere physically hits something (like the floor or a fire)
    private void OnCollisionEnter(Collision collision)
    {
        onCollision = true;

        // Optional: Make it explode instantly on impact instead of waiting for the timer
        if (grenadeActivated)
        {
            Explode();
        }
    }

    private void Explode()
    {
        // Prevent multiple explosions if timer and collision happen at the exact same time
        if (!grenadeActivated) return;
        grenadeActivated = false;

        // ===== ADDED: foam burst (white particles) =====
        SpawnFoamBurst(transform.position);
        // ===============================================

        // 1. Find all colliders within the explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        // 2. Check if any of them are Fires
        foreach (Collider hit in colliders)
        {
            Fire fire = hit.GetComponent<Fire>();
            if (fire != null)
            {
                // Instantly extinguish the fire by dropping its health
                fire.Health -= extinguishingPower;
            }
        }

        // 3. Destroy the grenade object after it pops
        Destroy(gameObject);
    }

    // ===== ADDED: creates the foam at the explosion position =====
    private void SpawnFoamBurst(Vector3 position)
    {
        if (foamBurstPrefab == null)
        {
            Debug.LogWarning("No foam burst prefab assigned on the grenade.");
            return;
        }

        // Created as a separate object: it keeps playing after the grenade is destroyed
        GameObject foam = Instantiate(foamBurstPrefab, position, Quaternion.identity);

        // Make sure the particles play, even if Play On Awake is unchecked
        ParticleSystem ps = foam.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Play(true);
            // Destroy once all particles are gone
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(foam, Mathf.Max(duration, foamLifetime));
        }
        else
        {
            Destroy(foam, foamLifetime);
        }
    }
    // =============================================================
}