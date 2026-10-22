using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Position Snap (grid)")]
    public float gridSize = 0.3f;
    public float heightStep = 5.0f;
    public float yOffset = 0f;
    public LayerMask snapSurfaces;

    [Header("Rotation (smart mode)")]
    [Tooltip("Spacing between allowed angles, in degrees.")]
    public float rotationStep = 10f;
    [Tooltip("Max rotation per axis in degrees, both directions from the reference. 0 locks the axis, 180 means unlimited.")]
    public Vector3 maxAngle = new Vector3(20f, 20f, 20f);
    [Tooltip("ON: limits measured from upright. OFF: from the orientation when smart mode started.")]
    public bool limitFromUpright = false;

    [Header("Platform-to-platform snap")]
    public bool enablePlatformSnap = true;
    [Tooltip("Layer of the SnapPoint trigger colliders.")]
    public LayerMask snapPointLayer;
    [Tooltip("Distance at which two snap points attract each other.")]
    public float snapRadius = 0.2f;
    [Tooltip("When snapped, also align the platform face-to-face (twist stays constrained to the rotation step). Max angle is ignored while snapped.")]
    public bool alignRotationOnSnap = true;

    [Header("Ghost")]
    public Material ghostMaterial;
    public Material ghostSnapMaterial;   // highlighted when snapped to another platform

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private GameObject ghostObject;
    private readonly List<MeshRenderer> ghostRenderers = new List<MeshRenderer>();
    private SnapPoint[] mySnapPoints;

    private bool originalTrackRotation;
    private bool wasSmart;
    private bool ghostIsSnapped;
    private Quaternion gripOffset;
    private Quaternion referenceRot;

    private void OnValidate()
    {
        rotationStep = Mathf.Max(1f, rotationStep);
        snapRadius = Mathf.Max(0.01f, snapRadius);
        maxAngle = new Vector3(
            Mathf.Clamp(maxAngle.x, 0f, 180f),
            Mathf.Clamp(maxAngle.y, 0f, 180f),
            Mathf.Clamp(maxAngle.z, 0f, 180f));
    }

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalTrackRotation = grabInteractable.trackRotation;
        mySnapPoints = GetComponentsInChildren<SnapPoint>();
        grabInteractable.selectExited.AddListener(OnRelease);
        BuildGhost();
    }

    private void BuildGhost()
    {
        ghostObject = new GameObject(gameObject.name + "_Ghost");

        // Copy every mesh with its pose relative to this object's root
        foreach (var mf in GetComponentsInChildren<MeshFilter>())
        {
            var part = new GameObject(mf.name);
            part.transform.SetParent(ghostObject.transform, false);
            part.transform.localPosition = transform.InverseTransformPoint(mf.transform.position);
            part.transform.localRotation = Quaternion.Inverse(transform.rotation) * mf.transform.rotation;

            Vector3 s = mf.transform.lossyScale;
            Vector3 r = transform.lossyScale;
            part.transform.localScale = new Vector3(s.x / r.x, s.y / r.y, s.z / r.z) * 1.02f;

            part.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
            var mr = part.AddComponent<MeshRenderer>();
            mr.sharedMaterial = ghostMaterial;
            ghostRenderers.Add(mr);
        }

        ghostObject.transform.localScale = transform.lossyScale;
        ghostObject.SetActive(false);
    }

    private void Update()
    {
        bool smart = grabInteractable.isSelected && SmartPlacementToggler.IsSmartModeActive;

        if (smart != wasSmart)
        {
            wasSmart = smart;
            grabInteractable.trackRotation = smart ? false : originalTrackRotation;

            if (smart)
            {
                Transform attach = grabInteractable.firstInteractorSelecting
                                   .GetAttachTransform(grabInteractable);
                gripOffset = Quaternion.Inverse(attach.rotation) * transform.rotation;
                referenceRot = limitFromUpright ? Quaternion.identity : transform.rotation;
            }
        }

        if (!smart && ghostObject.activeSelf)
            ghostObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!wasSmart) return;

        Transform attach = grabInteractable.firstInteractorSelecting
                           ?.GetAttachTransform(grabInteractable);
        if (attach == null) return;

        // 1. Stepped + limited rotation (free rotation of the hand -> notches)
        Quaternion free = attach.rotation * gripOffset;
        Vector3 e = (Quaternion.Inverse(referenceRot) * free).eulerAngles;
        float x = Mathf.DeltaAngle(0f, e.x);
        float y = Mathf.DeltaAngle(0f, e.y);
        float z = Mathf.DeltaAngle(0f, e.z);

        x = Mathf.Clamp(Mathf.Round(x / rotationStep) * rotationStep, -maxAngle.x, maxAngle.x);
        y = Mathf.Clamp(Mathf.Round(y / rotationStep) * rotationStep, -maxAngle.y, maxAngle.y);
        z = Mathf.Clamp(Mathf.Round(z / rotationStep) * rotationStep, -maxAngle.z, maxAngle.z);

        Quaternion steppedRot = referenceRot * Quaternion.Euler(x, y, z);
        transform.rotation = steppedRot;   // snap points follow, so queries below are up to date

        // 2. Platform-to-platform snap (has priority over the grid)
        Vector3 targetPos;
        Quaternion targetRot = steppedRot;
        bool snapped = enablePlatformSnap && TryFindPlatformSnap(out targetPos, out targetRot);

        if (!snapped)
        {
            // 3. Otherwise: grid snap
            targetPos = GridSnap(transform.position);
            targetRot = steppedRot;
        }
        else
        {
            targetPos = lastSnapPos;
        }

        ghostObject.transform.SetPositionAndRotation(targetPos, targetRot);
        if (!ghostObject.activeSelf) ghostObject.SetActive(true);

        if (snapped != ghostIsSnapped)
        {
            ghostIsSnapped = snapped;
            Material mat = (snapped && ghostSnapMaterial != null) ? ghostSnapMaterial : ghostMaterial;
            foreach (var r in ghostRenderers) r.sharedMaterial = mat;
        }
    }

    private Vector3 GridSnap(Vector3 pos)
    {
        float snappedX = Mathf.Round(pos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(pos.z / gridSize) * gridSize;
        float snappedY = pos.y;

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100.0f, snapSurfaces))
        {
            float heightAboveFloor = pos.y - hit.point.y;
            float snappedHeightOffset = Mathf.Round(heightAboveFloor / heightStep) * heightStep;
            snappedHeightOffset = Mathf.Max(0, snappedHeightOffset);
            snappedY = hit.point.y + snappedHeightOffset + yOffset;
        }
        return new Vector3(snappedX, snappedY, snappedZ);
    }

    private Vector3 lastSnapPos;

    private bool TryFindPlatformSnap(out Vector3 pos, out Quaternion rot)
    {
        pos = default;
        rot = transform.rotation;

        Transform bestMine = null;
        SnapPoint bestOther = null;
        float bestDist = float.MaxValue;

        // Closest pair between my snap points and the other platforms' snap points
        foreach (var mine in mySnapPoints)
        {
            var hits = Physics.OverlapSphere(mine.transform.position, snapRadius,
                                             snapPointLayer, QueryTriggerInteraction.Collide);
            foreach (var h in hits)
            {
                var other = h.GetComponentInParent<SnapPoint>();
                if (other == null || other.transform.IsChildOf(transform)) continue;

                float d = (other.transform.position - mine.transform.position).sqrMagnitude;
                if (d < bestDist) { bestDist = d; bestMine = mine.transform; bestOther = other; }
            }
        }
        if (bestOther == null) return false;

        Quaternion rootRot = transform.rotation;

        if (alignRotationOnSnap)
        {
            // My face must look the opposite way of the target face (same red axis)
            Quaternion baseRot = bestOther.transform.rotation * Quaternion.AngleAxis(180f, Vector3.right);
            Vector3 up = baseRot * Vector3.up;

            // Free twist around the contact normal, constrained to the step
            Vector3 refFwd = baseRot * Vector3.forward;
            Vector3 curFwd = Vector3.ProjectOnPlane(bestMine.rotation * Vector3.forward, up);
            float twist = curFwd.sqrMagnitude > 1e-6f ? Vector3.SignedAngle(refFwd, curFwd, up) : 0f;
            twist = Mathf.Round(twist / rotationStep) * rotationStep;
            Quaternion snapRot = Quaternion.AngleAxis(twist, up) * baseRot;

            Quaternion mineLocalRot = Quaternion.Inverse(transform.rotation) * bestMine.rotation;
            rootRot = snapRot * Quaternion.Inverse(mineLocalRot);
        }

        // Offset of my snap point from my root, measured in the unrotated frame
        Vector3 offset = Quaternion.Inverse(transform.rotation) * (bestMine.position - transform.position);

        rot = rootRot;
        lastSnapPos = bestOther.transform.position - rootRot * offset;
        pos = lastSnapPos;
        return true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (wasSmart && ghostObject.activeSelf)
        {
            transform.SetPositionAndRotation(ghostObject.transform.position,
                                             ghostObject.transform.rotation);
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = Vector3.zero;
#else
                rb.velocity = Vector3.zero;
#endif
                rb.angularVelocity = Vector3.zero;
            }
        }

        grabInteractable.trackRotation = originalTrackRotation;
        wasSmart = false;
        ghostIsSnapped = false;
        foreach (var r in ghostRenderers) r.sharedMaterial = ghostMaterial;
        ghostObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null) grabInteractable.selectExited.RemoveListener(OnRelease);
        if (ghostObject != null) Destroy(ghostObject);
    }
}