using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
public class VRSnapPlacement : MonoBehaviour
{
    [Header("Snap Settings")]
    public float gridSize = 0.3f;    // The width of your platforms
    public float rotationStep = 10f; // Angle constraints
    public Material ghostMaterial;

    private XRGrabInteractable grabInteractable;
    private GameObject ghostObject;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.selectExited.AddListener(OnRelease);

        // Auto-generate the Ghost visual
        ghostObject = new GameObject(gameObject.name + "_Ghost");
        MeshFilter mf = ghostObject.AddComponent<MeshFilter>();
        MeshRenderer mr = ghostObject.AddComponent<MeshRenderer>();

        MeshFilter myMf = GetComponent<MeshFilter>();
        if (myMf != null) mf.sharedMesh = myMf.sharedMesh;
        mr.material = ghostMaterial;

        ghostObject.transform.localScale = transform.localScale;
        ghostObject.SetActive(false);
    }

    private void Update()
    {
        // Hide ghost if not held or if Smart Mode is off
        if (!grabInteractable.isSelected || !SmartPlacementToggler.IsSmartModeActive)
        {
            if (ghostObject.activeSelf) ghostObject.SetActive(false);
            return;
        }

        if (!ghostObject.activeSelf) ghostObject.SetActive(true);

        // 1. Grid Snap (Side-by-side alignment)
        Vector3 currentPos = transform.position;
        Vector3 snappedPos = new Vector3(
            Mathf.Round(currentPos.x / gridSize) * gridSize,
            Mathf.Round(currentPos.y / gridSize) * gridSize,
            Mathf.Round(currentPos.z / gridSize) * gridSize
        );
        ghostObject.transform.position = snappedPos;

        // 2. Angle Snap
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