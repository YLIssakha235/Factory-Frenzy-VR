using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Smooth keyboard movement for the FlyCam, with acceleration and deceleration.
/// WASD / arrows = move, Q / E = down / up, Shift = faster, mouse wheel = change speed.
/// </summary>
public class FlyCamMove : MonoBehaviour
{
    [SerializeField] private float speed = 3f;          // metres per second
    [SerializeField] private float fastMultiplier = 3f;
    [Tooltip("How quickly it speeds up / slows down. Lower = more gliding.")]
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 20f;

    private Vector3 _velocity;

    private void Update()
    {
        Vector3 input = ReadMoveInput();
        if (input.sqrMagnitude > 1f) input.Normalize();   // no faster diagonals

        speed = Mathf.Clamp(speed * (1f + ReadScroll() * 0.1f), minSpeed, maxSpeed);
        float s = speed * (FastHeld() ? fastMultiplier : 1f);

        Vector3 targetVelocity = (transform.right * input.x
                                + Vector3.up * input.y
                                + transform.forward * input.z) * s;

        float t = 1f - Mathf.Exp(-acceleration * Time.deltaTime);
        _velocity = Vector3.Lerp(_velocity, targetVelocity, t);
        transform.position += _velocity * Time.deltaTime;
    }

    private void OnDisable() => _velocity = Vector3.zero;

    private Vector3 ReadMoveInput()
    {
        Vector3 v = Vector3.zero;
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k == null) return v;
        if (k.wKey.isPressed || k.upArrowKey.isPressed)    v.z += 1f;
        if (k.sKey.isPressed || k.downArrowKey.isPressed)  v.z -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) v.x += 1f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed)  v.x -= 1f;
        if (k.eKey.isPressed) v.y += 1f;
        if (k.qKey.isPressed) v.y -= 1f;
#else
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    v.z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  v.z -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) v.x += 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  v.x -= 1f;
        if (Input.GetKey(KeyCode.E)) v.y += 1f;
        if (Input.GetKey(KeyCode.Q)) v.y -= 1f;
#endif
        return v;
    }

    private bool FastHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.shiftKey.isPressed;
#else
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
    }

    private float ReadScroll()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return 0f;
        float y = Mouse.current.scroll.ReadValue().y;
        return y == 0f ? 0f : Mathf.Sign(y);
#else
        return Input.mouseScrollDelta.y;
#endif
    }
}
