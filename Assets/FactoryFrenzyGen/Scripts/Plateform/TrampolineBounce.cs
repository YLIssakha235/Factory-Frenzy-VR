using UnityEngine;

public class TrampolineBounce : MonoBehaviour
{
    public float bounceForce = 12f;

    private TrampolineFX trampolineFX;

    void Start()
    {
        trampolineFX = GetComponentInParent<TrampolineFX>();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerPhysics player = other.GetComponent<PlayerPhysics>();

        if (player == null) return;

        player.AddImpulse(Vector3.up * bounceForce);

        if (trampolineFX != null)
        {
            trampolineFX.Bounce();
        }
    }
}