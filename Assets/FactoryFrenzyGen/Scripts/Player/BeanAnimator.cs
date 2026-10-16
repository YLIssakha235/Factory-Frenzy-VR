using UnityEngine;

// Animates the bean character with code (no animation files).
// Put it on the PlayerBean object (child of PlayerArmature).
public class BeanAnimator : MonoBehaviour
{
    Transform visual, armL, armR, legL, legR;
    CharacterController controller;

    float walkTime = 0;        // advances when we walk, used for the leg swing
    float squash = 0;          // squash when we land
    float hitTime = -1;        // -1 = not hit
    Vector3 hitAxis;           // the axis the bean tumbles around
    float celebrateTime = -1;  // -1 = not celebrating
    bool wasOnGround = true;
    Vector3 lastPosition;      // where the bean was in the last frame

    void Start()
    {
        controller = GetComponentInParent<CharacterController>(); // can be null in a test scene
        lastPosition = transform.position;
        visual = FindChild("Visual");
        armL = FindChild("ArmL");
        armR = FindChild("ArmR");
        legL = FindChild("LegL");
        legR = FindChild("LegR");

        if (visual == null || armL == null || armR == null || legL == null || legR == null)
        {
            Debug.LogWarning("BeanAnimator: put this script on the PlayerBean (it needs Visual, ArmL, ArmR, LegL, LegR).");
        }
    }

    // looks for a child with this name, at any depth
    Transform FindChild(string childName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == childName) return t;
        }
        return null;
    }

    // Called by PlayerPhysics when a trap pushes us
    public void OnPushed(Vector3 force)
    {
        Vector3 sideways = new Vector3(force.x, 0, force.z);
        if (sideways.magnitude > 8) // only big sideways hits (not the trampoline)
        {
            hitTime = 0;
            squash = 0.35f; // squashed by the impact

            // tumble in the direction of the push (in the bean's own space)
            Vector3 localPush = transform.InverseTransformDirection(sideways.normalized);
            hitAxis = Vector3.Cross(Vector3.up, localPush);
        }
    }

    // Call it when the player finishes the race
    public void Celebrate()
    {
        celebrateTime = 0;
    }

    void Update()
    {
        if (visual == null || armL == null || armR == null || legL == null || legR == null) return;

        // our real speed = how far we moved since the last frame
        // (so when the player is frozen, the speed is 0 and the bean stops walking)
        Vector3 velocity = Vector3.zero;
        if (Time.deltaTime > 0)
        {
            velocity = (transform.position - lastPosition) / Time.deltaTime;
        }
        lastPosition = transform.position;

        bool onGround = true;
        if (controller != null && controller.enabled)
        {
            onGround = controller.isGrounded;
        }
        float speed = new Vector3(velocity.x, 0, velocity.z).magnitude;

        // 1. WALK / RUN: legs and arms swing, faster when we go faster
        walkTime += Time.deltaTime * speed * 2.5f;
        float swing = Mathf.Sin(walkTime) * Mathf.Min(speed * 10, 45);
        float armLeft = -swing;
        float armRight = swing;

        // 2. IN THE AIR: arms up
        if (!onGround)
        {
            armLeft = -150;
            armRight = -150;
        }

        // 3. LANDING: squash the body a little
        if (onGround && !wasOnGround)
        {
            squash = 0.3f;
        }
        wasOnGround = onGround;
        squash = Mathf.MoveTowards(squash, 0, Time.deltaTime * 1.5f);

        // 4. CELEBRATE: hop and wave for 3 seconds
        float hop = 0;
        if (celebrateTime >= 0)
        {
            celebrateTime += Time.deltaTime;
            hop = Mathf.Abs(Mathf.Sin(celebrateTime * 6)) * 0.4f;
            armLeft = -160 + Mathf.Sin(celebrateTime * 14) * 20;
            armRight = -160 - Mathf.Sin(celebrateTime * 14) * 20;
            if (celebrateTime > 3) celebrateTime = -1;
        }

        // 5. HIT: tumble in the push direction with arms and legs flailing, then a dizzy wobble
        Quaternion bodyRotation = Quaternion.identity;
        if (hitTime >= 0)
        {
            hitTime += Time.deltaTime;

            if (hitTime < 0.8f)
            {
                // one full turn: fast at the start, slower at the end
                float u = hitTime / 0.8f;
                float angle = 360 * (1 - (1 - u) * (1 - u));
                bodyRotation = Quaternion.AngleAxis(angle, hitAxis);

                // arms and legs flail
                armLeft = -90 + Mathf.Sin(hitTime * 40) * 60;
                armRight = -90 - Mathf.Sin(hitTime * 40) * 60;
                swing = Mathf.Sin(hitTime * 35) * 50;
            }
            else if (hitTime < 1.4f)
            {
                // dizzy: wobble left and right, smaller and smaller
                float fade = 1 - (hitTime - 0.8f) / 0.6f;
                bodyRotation = Quaternion.Euler(0, 0, Mathf.Sin(hitTime * 20) * 15 * fade);
            }
            else
            {
                hitTime = -1; // finished
            }
        }

        // apply everything
        legL.localRotation = Quaternion.Euler(swing, 0, 0);
        legR.localRotation = Quaternion.Euler(-swing, 0, 0);
        armL.localRotation = Quaternion.Euler(armLeft, 0, 0);
        armR.localRotation = Quaternion.Euler(armRight, 0, 0);
        visual.localScale = new Vector3(1 + squash, 1 - squash, 1 + squash);

        // turn around the belly (0.85 m high), not around the feet
        Vector3 belly = new Vector3(0, 0.85f, 0);
        visual.localRotation = bodyRotation;
        visual.localPosition = belly - bodyRotation * belly + new Vector3(0, hop, 0);
    }
}