using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class LockTool : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private XRInteractorLineVisual lineVisual;
    [SerializeField] private XRBaseController controller;
    [SerializeField] private AudioClip toggleSound;

    [Header("Inputs")]
    [Tooltip("Button to toggle Lock Mode on/off")]
    [SerializeField] private InputActionProperty toggleLockModeAction;

    [Tooltip("Trigger to confirm lock/unlock")]
    [SerializeField] private InputActionProperty confirmLockAction;

    [Header("Visual Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lockHoverColor = Color.yellow;

    private AudioSource audioSource;
    private Gradient normalGradient;
    private Gradient lockHoverGradient;

    private bool isLockModeActive = false;
    private bool isCurrentlyHovering = false;
    private System.Collections.Generic.Dictionary<MeshRenderer, Color> originalColors = new System.Collections.Generic.Dictionary<MeshRenderer, Color>();

    private void Awake()
    {
        if (lineVisual == null) lineVisual = GetComponent<XRInteractorLineVisual>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        normalGradient = CreateSolidGradient(normalColor);
        lockHoverGradient = CreateSolidGradient(lockHoverColor);
        SetRayColor(false);
    }

    private void OnEnable()
    {
        toggleLockModeAction.action?.Enable();
        confirmLockAction.action?.Enable();
    }

    private void OnDisable()
    {
        toggleLockModeAction.action?.Disable();
        confirmLockAction.action?.Disable();
        SetRayColor(false);
    }

    private void Update()
    {
        if (toggleLockModeAction.action != null && toggleLockModeAction.action.WasPressedThisFrame())
        {
            isLockModeActive = !isLockModeActive;
            controller?.SendHapticImpulse(0.3f, 0.1f);
            if (!isLockModeActive) SetRayColor(false);
        }

        if (!isLockModeActive) return;

        UpdateRayHoverState();

        if (confirmLockAction.action != null && confirmLockAction.action.WasPressedThisFrame())
        {
            TryToggleLockTarget();
        }
    }

    private void UpdateRayHoverState()
    {
        if (rayInteractor == null || lineVisual == null) return;
        bool targetFound = false;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            EditableObject target = hit.collider.GetComponentInParent<EditableObject>();
            if (target != null) targetFound = true;
        }

        if (targetFound != isCurrentlyHovering) SetRayColor(targetFound);
    }

    private void TryToggleLockTarget()
    {
        if (rayInteractor == null) return;

        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            EditableObject target = hit.collider.GetComponentInParent<EditableObject>();
            if (target == null) return;

            XRGrabInteractable grabInteractable = target.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                controller?.SendHapticImpulse(0.8f, 0.15f);
                if (toggleSound != null && audioSource != null) audioSource.PlayOneShot(toggleSound);

                // Toggle the grab interactable
                bool isCurrentlyLocked = !grabInteractable.enabled;
                grabInteractable.enabled = isCurrentlyLocked;

                MeshRenderer renderer = target.GetComponentInChildren<MeshRenderer>();
                if (renderer != null)
                {
                    if (!isCurrentlyLocked) // We just locked it
                    {
                        // Save the original color to memory before making it gray
                        if (!originalColors.ContainsKey(renderer))
                        {
                            originalColors[renderer] = renderer.material.color;
                        }
                        renderer.material.color = Color.gray;
                    }
                    else // We just unlocked it
                    {
                        // Restore the original color from memory
                        if (originalColors.ContainsKey(renderer))
                        {
                            renderer.material.color = originalColors[renderer];
                        }
                    }
                }
            }
        }
    }

    private void SetRayColor(bool isHoveringTarget)
    {
        isCurrentlyHovering = isHoveringTarget;
        if (lineVisual != null)
        {
            lineVisual.validColorGradient = isCurrentlyHovering ? lockHoverGradient : normalGradient;
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