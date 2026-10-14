using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;

public class TiagoRepairManager : MonoBehaviour
{
    [Header("References")]
    public GameObject robotHead;
    public NavMeshAgent robotAgent;

    Transform originalParent;
    Vector3 originalLocalPos;
    Quaternion originalLocalRot;

    void Awake()
    {
        // Captured at startup, before HeadExplosion detaches the head
        originalParent = robotHead.transform.parent;
        originalLocalPos = robotHead.transform.localPosition;
        originalLocalRot = robotHead.transform.localRotation;
    }

    // Called from the socket's Select Entered event
    public void OnHeadReattached()
    {
        StartCoroutine(FinishReattach());
    }

    IEnumerator FinishReattach()
    {
        // Let the socket finish its attach ease-in (0.15s)
        yield return new WaitForSeconds(0.3f);

        // Release the grab first, so it can't touch the Rigidbody afterwards
        robotHead.GetComponent<XRGrabInteractable>().enabled = false;
        yield return null;

        // Restore the exact original place on the robot
        var t = robotHead.transform;
        t.SetParent(originalParent);
        t.localPosition = originalLocalPos;
        t.localRotation = originalLocalRot;

        // Set kinematic last
        var rb = robotHead.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (robotAgent != null)
            robotAgent.isStopped = false;
    }
}