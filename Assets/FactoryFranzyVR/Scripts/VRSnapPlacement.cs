using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Snap Settings")]
    public float gridSize = 0.3f;
    public float heightStep = 5.0f;
    public float yOffset = 0f;
    public float rotationStep = 10f;
    public LayerMask snapSurfaces;
    public Material ghostMaterial;

    private XRGrabInteractable grabInteractable;
    private GameObject ghostObject;

    private bool originalTrackRotation;
    private bool wasSmart;
    private Quaternion gripOffset;   // object rotation relative to the hand when smart mode starts

    private void OnValidate()
    {
        rotationStep = Mathf.Max(1f, rotationStep);
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
            }
        }

        if (!smart)
        {
            if (ghostObject.activeSelf) ghostObject.SetActive(false);
            return;
        }

        if (!ghostObject.activeSelf) ghostObject.SetActive(true);

        // Position (unchanged)
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

        // Rotation the object WOULD have if it followed the hand freely
        Transform attach = grabInteractable.firstInteractorSelecting
                           ?.GetAttachTransform(grabInteractable);
        if (attach == null) return;

        Vector3 e = (attach.rotation * gripOffset).eulerAngles;
        Quaternion snapped = Quaternion.Euler(
            Mathf.Round(e.x / rotationStep) * rotationStep,
            Mathf.Round(e.y / rotationStep) * rotationStep,
            Mathf.Round(e.z / rotationStep) * rotationStep
        );

        // The held object itself now rotates in steps, and the ghost matches it
        transform.rotation = snapped;
        ghostObject.transform.rotation = snapped;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        if (SmartPlacementToggler.IsSmartModeActive)
        {
            transform.position = ghostObject.transform.position;
            transform.rotation = ghostObject.transform.rotation;
        }
        ghostObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null) grabInteractable.selectExited.RemoveListener(OnRelease);
        if (ghostObject != null) Destroy(ghostObject);
    }
}