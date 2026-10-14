using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// First-person "floating head" look: the mouse always turns the FlyCam (like an FPS game).
/// A crosshair shows where the trainer is aiming; left click places a marker there.
/// Escape frees the cursor (to click UI buttons), Escape again goes back to looking.
/// </summary>
[RequireComponent(typeof(Camera))]
public class FlyCamLook : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2f;
    [Tooltip("Higher = more responsive, lower = smoother. 0 = no smoothing.")]
    [SerializeField] private float smoothing = 15f;
    [Tooltip("Ticked: hold the right mouse button to look. Unticked: the mouse always looks (FPS style).")]
    [SerializeField] private bool holdRightClick = false;
    [SerializeField] private bool invertY = false;
    [SerializeField] private float minPitch = -89f;
    [SerializeField] private float maxPitch = 89f;

    [Header("Crosshair")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float crosshairSize = 10f;

    private Camera _cam;
    private float _targetYaw, _targetPitch, _yaw, _pitch;
    private bool _cursorFreed;

    public bool IsLooking { get; private set; }

    private void Awake() => _cam = GetComponent<Camera>();

    private void OnEnable()
    {
        Vector3 e = transform.eulerAngles;
        _yaw = _targetYaw = e.y;
        _pitch = _targetPitch = e.x > 180f ? e.x - 360f : e.x;
        _cursorFreed = false;
    }

    private void Update()
    {
        if (!holdRightClick && EscapePressed()) _cursorFreed = !_cursorFreed;

        IsLooking = holdRightClick ? RightButtonHeld() : !_cursorFreed;
        Cursor.lockState = IsLooking ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !IsLooking;

        if (IsLooking)
        {
            Vector2 delta = MouseDelta() * sensitivity;
            _targetYaw += delta.x;
            _targetPitch += invertY ? delta.y : -delta.y;
            _targetPitch = Mathf.Clamp(_targetPitch, minPitch, maxPitch);
        }

        float t = smoothing > 0f ? 1f - Mathf.Exp(-smoothing * Time.deltaTime) : 1f;
        _yaw = Mathf.Lerp(_yaw, _targetYaw, t);
        _pitch = Mathf.Lerp(_pitch, _targetPitch, t);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void OnDisable()
    {
        IsLooking = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Small "+" in the middle of the FlyCam's view (also works in picture-in-picture).
    private void OnGUI()
    {
        if (!showCrosshair || !IsLooking || !_cam.enabled) return;
        Rect r = _cam.pixelRect;
        float cx = r.x + r.width / 2f;
        float cy = Screen.height - (r.y + r.height / 2f);   // GUI y goes top-down
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - crosshairSize, cy - 1f, crosshairSize * 2f, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1f, cy - crosshairSize, 2f, crosshairSize * 2f), Texture2D.whiteTexture);
    }

    private bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private bool RightButtonHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    private Vector2 MouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue() * 0.1f : Vector2.zero;
#else
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
#endif
    }
}
