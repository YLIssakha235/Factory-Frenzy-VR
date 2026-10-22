using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Snap Settings")]
    public float gridSize = 0.3f;
    public float heightStep = 5.0f;
    public float yOffset = 0f;       // NEW: Fine-tune vertical resting position
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

        Vector3 currentPos = transform.position;
        float snappedX = Mathf.Round(currentPos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(currentPos.z / gridSize) * gridSize;
        float snappedY = currentPos.y;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 100.0f, snapSurfaces))
        {
            float heightAboveFloor = currentPos.y - hit.point.y;
            float snappedHeightOffset = Mathf.Round(heightAboveFloor / heightStep) * heightStep;
            snappedHeightOffset = Mathf.Max(0, snappedHeightOffset);

            // FIX: Replaced automatic scale math with a manual offset
            snappedY = hit.point.y + snappedHeightOffset + yOffset;
        }

        ghostObject.transform.position = new Vector3(snappedX, snappedY, snappedZ);

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