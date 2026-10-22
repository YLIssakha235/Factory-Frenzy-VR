using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Position Snap")]
    public float gridSize = 0.3f;
    public float heightStep = 5.0f;
    public float yOffset = 0f;
    public LayerMask snapSurfaces;

    [Header("Rotation (smart mode)")]
    [Tooltip("Spacing between allowed angles, in degrees.")]
    public float rotationStep = 10f;
    [Tooltip("Max rotation per axis in degrees, in both directions from the reference. 0 locks the axis, 180 means unlimited.")]
    public Vector3 maxAngle = new Vector3(20f, 20f, 20f);
    [Tooltip("ON: limits are measured from upright. OFF: from the orientation the platform had when smart mode started.")]
    public bool limitFromUpright = false;

    [Header("Ghost")]
    public Material ghostMaterial;

    private XRGrabInteractable grabInteractable;
    private GameObject ghostObject;

    private bool originalTrackRotation;
    private bool wasSmart;
    private Quaternion gripOffset;     // object rotation relative to the hand when smart mode starts
    private Quaternion referenceRot;   // center of the allowed rotation range

    private void OnValidate()
    {
        rotationStep = Mathf.Max(1f, rotationStep);
        maxAngle = new Vector3(
            Mathf.Clamp(maxAngle.x, 0f, 180f),
            Mathf.Clamp(maxAngle.y, 0f, 180f),
            Mathf.Clamp(maxAngle.z, 0f, 180f));
    }

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        originalTrackRotation = grabInteractable.trackRotation;
        grabInteractable.selectExited.AddListener(OnRelease);

        ghostObject = new GameObject(gameObject.name + "_Ghost");
        MeshFilter mf = ghostObject.AddComponent<MeshFilter>();
        MeshRenderer mr = ghostObject.AddComponent<MeshRenderer>();

        MeshFilter myMf = GetComponentInChildren<MeshFilter>();
        if (myMf != null) mf.sharedMesh = myMf.sharedMesh;

        mr.material = ghostMaterial;
        ghostObject.transform.localScale = transform.localScale * 1.02f;
        ghostObject.SetActive(false);
    }

    private void Update()
    {
        bool smart = grabInteractable.isSelected && SmartPlacementToggler.IsSmartModeActive;

        // Entering / leaving smart mode
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

        if (!smart)
        {
            if (ghostObject.activeSelf) ghostObject.SetActive(false);
            return;
        }

        if (!ghostObject.activeSelf) ghostObject.SetActive(true);

        // Position snap
        Vector3 currentPos = transform.position;
        float snappedX = Mathf.Round(currentPos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(currentPos.z / gridSize) * gridSize;
        float snappedY = currentPos.y;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100.0f, snapSurfaces))
        {
            float heightAboveFloor = currentPos.y - hit.point.y;
            float snappedHeightOffset = Mathf.Round(heightAboveFloor / heightStep) * heightStep;
            snappedHeightOffset = Mathf.Max(0, snappedHeightOffset);
            snappedY = hit.point.y + snappedHeightOffset + yOffset;
        }

        ghostObject.transform.position = new Vector3(snappedX, snappedY, snappedZ);
    }

    private void LateUpdate()
    {
        if (!wasSmart) return;

        Transform attach = grabInteractable.firstInteractorSelecting
                           ?.GetAttachTransform(grabInteractable);
        if (attach == null) return;

        // Where the object would be if it followed the hand freely
        Quaternion free = attach.rotation * gripOffset;

        // Rotation relative to the reference, as signed angles (-180..180)
        Vector3 e = (Quaternion.Inverse(referenceRot) * free).eulerAngles;
        float x = Mathf.DeltaAngle(0f, e.x);
        float y = Mathf.DeltaAngle(0f, e.y);
        float z = Mathf.DeltaAngle(0f, e.z);

        // Round to the step, then clamp so the limit is never exceeded
        x = Mathf.Clamp(Mathf.Round(x / rotationStep) * rotationStep, -maxAngle.x, maxAngle.x);
        y = Mathf.Clamp(Mathf.Round(y / rotationStep) * rotationStep, -maxAngle.y, maxAngle.y);
        z = Mathf.Clamp(Mathf.Round(z / rotationStep) * rotationStep, -maxAngle.z, maxAngle.z);

        Quaternion result = referenceRot * Quaternion.Euler(x, y, z);

        transform.rotation = result;
        ghostObject.transform.rotation = result;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (wasSmart)
        {
            transform.position = ghostObject.transform.position;
            transform.rotation = ghostObject.transform.rotation;
        }

        // Always restore normal grab behavior
        grabInteractable.trackRotation = originalTrackRotation;
        wasSmart = false;
        ghostObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null) grabInteractable.selectExited.RemoveListener(OnRelease);
        if (ghostObject != null) Destroy(ghostObject);
    }
}