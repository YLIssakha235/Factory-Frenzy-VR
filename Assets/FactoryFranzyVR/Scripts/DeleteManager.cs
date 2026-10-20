using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class DeleteManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private XRInteractorLineVisual lineVisual;
    [SerializeField] private XRBaseController controller;
    [SerializeField] private AudioClip deleteSound;

    [Header("Inputs")]
    [Tooltip("Button to toggle Delete Mode on/off (e.g., B button or F key)")]
    [SerializeField] private InputActionProperty toggleDeleteModeAction;

    [Tooltip("Trigger to confirm deletion while in Delete Mode")]
    [SerializeField] private InputActionProperty confirmDeleteAction;

    [Header("Visual Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color deleteHoverColor = Color.red;

    private AudioSource audioSource;
    private Gradient normalGradient;
    private Gradient deleteHoverGradient;

    private bool isDeleteModeActive = false;
    private bool isCurrentlyHovering = false;

    private void Awake()
    {
        if (lineVisual == null)
            lineVisual = GetComponent<XRInteractorLineVisual>();

        // Dedicated AudioSource avoids instantiation latency on delete
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D clean playback directly in ears, or 1f for 3D positional
        }

        normalGradient = CreateSolidGradient(normalColor);
        deleteHoverGradient = CreateSolidGradient(deleteHoverColor);

        SetRayColor(false);
    }

    private void OnEnable()
    {
        toggleDeleteModeAction.action?.Enable();
        confirmDeleteAction.action?.Enable();
    }

    private void OnDisable()
    {
        toggleDeleteModeAction.action?.Disable();
        confirmDeleteAction.action?.Disable();
        SetRayColor(false);
    }

    private void Update()
    {
        // 1. Toggle Delete Mode on/off
        if (toggleDeleteModeAction.action != null && toggleDeleteModeAction.action.WasPressedThisFrame())
        {
            isDeleteModeActive = !isDeleteModeActive;
            controller?.SendHapticImpulse(0.3f, 0.1f);

            if (!isDeleteModeActive)
            {
                SetRayColor(false);
            }
        }

        if (!isDeleteModeActive) return;

        // 2. Continuously check what the ray is pointing at
        UpdateRayHoverState();

        // 3. Confirm deletion on trigger pull
        if (confirmDeleteAction.action != null && confirmDeleteAction.action.WasPressedThisFrame())
        {
            TryDeleteTarget();
        }
    }

    private void UpdateRayHoverState()
    {
        if (rayInteractor == null || lineVisual == null) return;

        bool targetFound = false;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            EditableObject target = hit.collider.GetComponentInParent<EditableObject>();
            if (target != null)
            {
                targetFound = true;
            }
        }

        if (targetFound != isCurrentlyHovering)
        {
            SetRayColor(targetFound);
        }
    }

    private void TryDeleteTarget()
    {
        if (rayInteractor == null) return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            EditableObject target = hit.collider.GetComponentInParent<EditableObject>();
            if (target == null) return;

            controller?.SendHapticImpulse(0.8f, 0.15f);

            // Play sound immediately before object destruction
            if (deleteSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deleteSound);
            }

            LevelManager.Instance?.UnregisterObject(target);
            Destroy(target.gameObject);

            SetRayColor(false);
        }
    }

    private void SetRayColor(bool isHoveringTarget)
    {
        isCurrentlyHovering = isHoveringTarget;

        if (lineVisual != null)
        {
            lineVisual.validColorGradient = isCurrentlyHovering ? deleteHoverGradient : normalGradient;
        }
    }

    private Gradient CreateSolidGradient(Color col)
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(col, 0.0f), new GradientColorKey(col, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(col.a, 0.0f), new GradientAlphaKey(col.a, 1.0f) }
        );
        return g;
    }
}