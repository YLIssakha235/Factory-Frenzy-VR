using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Snap Settings")]
    public float gridSize = 0.3f;
    public float heightStep = 5.0f;  // NEW: Controls the vertical snap intervals (0, 5, 10, 15...)
    public float rotationStep = 10f;
    public LayerMask snapSurfaces;
    public Material ghostMaterial;

    private XRGrabInteractable grabInteractable;
    private GameObject ghostObject;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
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
        if (!grabInteractable.isSelected || !SmartPlacementToggler.IsSmartModeActive)
        {
            if (ghostObject.activeSelf) ghostObject.SetActive(false);
            return;
        }

        if (!ghostObject.activeSelf) ghostObject.SetActive(true);

        // 1. Horizontal Grid Snap
        Vector3 currentPos = transform.position;
        float snappedX = Mathf.Round(currentPos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(currentPos.z / gridSize) * gridSize;
        float snappedY = currentPos.y;

        // 2. Vertical Grid Snap based on Floor Distance
        // Increased raycast distance to 100 to easily find the floor from high up
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100.0f, snapSurfaces))
        {
            // Calculate how high the object is currently held above the floor
            float heightAboveFloor = currentPos.y - hit.point.y;

            // Round that height to the nearest step (e.g., 0, 5, 10, 15)
            float snappedHeightOffset = Mathf.Round(heightAboveFloor / heightStep) * heightStep;

            // Prevent the object from snapping underground
            snappedHeightOffset = Mathf.Max(0, snappedHeightOffset);

            // Final Y = Base floor height + height step + half object thickness
            snappedY = hit.point.y + snappedHeightOffset + (transform.localScale.y / 2f);
        }

        ghostObject.transform.position = new Vector3(snappedX, snappedY, snappedZ);

        // 3. Angle Snap
        Vector3 currentEuler = transform.eulerAngles;
        Vector3 snappedEuler = new Vector3(
            Mathf.Round(currentEuler.x / rotationStep) * rotationStep,
            Mathf.Round(currentEuler.y / rotationStep) * rotationStep,
            Mathf.Round(currentEuler.z / rotationStep) * rotationStep
        );
        ghostObject.transform.rotation = Quaternion.Euler(snappedEuler);
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