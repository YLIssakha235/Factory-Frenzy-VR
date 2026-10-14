// Script created by LINEACT CESI
// Author: FlyCam
// Modified: input read in Update (once per frame), no time scaling on mouse delta,
// clamped pitch, inputs reset when the window loses focus.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace intervales.utils
{
    public class FlyCam : MonoBehaviour
    {
        #region own property, use camelLowerCase for private property, use CamelUpperCase for public property 
        private Vector2 look;
        private Vector2 move;
        private InputAction lookAction, moveAction, fire2Action, fire3Action, moveFastAction;
        private float pitch;

        public Camera cam;

        [Tooltip("Degrees per mouse pixel. Try 0.1.")]
        public float RotXSensitivity = 0.1f;
        public float RotYSensitivity = 0.1f;

        [Tooltip("Metres per second.")]
        public float MoveXSensitivity = 2.0f;
        public float MoveYSensitivity = 2.0f;

        [Tooltip("Metres per mouse pixel (middle-click pan). Try 0.01.")]
        public float AltXSensitivity = 0.01f;
        public float AltYSensitivity = 0.01f;
        public float MoveFastMultiplier = 2.0f;

        public float MinPitch = -85f;
        public float MaxPitch = 85f;

        public InputActionAsset inputActions;
        #endregion

        #region Unity Events
        void Start()
        {
            if (inputActions == null) inputActions = GetComponent<PlayerInput>().actions;

            // Private copy of the actions, limited to keyboard + mouse only.
            // This way the VR controllers (and gamepads) can never move the FlyCam,
            // and the VR rig's own input actions are not affected.
            inputActions = Instantiate(inputActions);
            var devices = new List<InputDevice>();
            if (Keyboard.current != null) devices.Add(Keyboard.current);
            if (Mouse.current != null) devices.Add(Mouse.current);
            inputActions.devices = devices.ToArray();
            inputActions.Enable();

            look = Vector2.zero;
            cam = transform.Find("Camera").gameObject.GetComponent<Camera>();

            lookAction = inputActions.FindAction("Look");
            moveAction = inputActions.FindAction("Move");
            fire2Action = inputActions.FindAction("Fire2");
            fire3Action = inputActions.FindAction("Fire3");
            moveFastAction = inputActions.FindAction("MoveFast");

            float x = cam.transform.localEulerAngles.x;
            pitch = x > 180f ? x - 360f : x;
        }

        // Everything runs once per frame now (it was in FixedUpdate before).
        void Update()
        {
            if (cam == null) return;

            // Read the actions directly every frame: no value can stay "stuck".
            if (lookAction != null) look = lookAction.ReadValue<Vector2>();
            if (moveAction != null) move = moveAction.ReadValue<Vector2>();

            float multiplier = moveFastAction != null && moveFastAction.IsPressed() ? MoveFastMultiplier : 1;

            // Free look (right click). Mouse delta is already a distance: no Time.deltaTime, no fast multiplier.
            if (look != Vector2.zero && fire2Action != null && fire2Action.IsPressed())
            {
                transform.Rotate(0, look.x * RotYSensitivity, 0, Space.World);
                pitch = Mathf.Clamp(pitch - look.y * RotXSensitivity, MinPitch, MaxPitch);
                cam.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            }

            // Pan (middle click).
            if (look != Vector2.zero && fire3Action != null && fire3Action.IsPressed())
            {
                var v = Vector3.zero;
                v.x = -look.x * AltXSensitivity * multiplier;
                v.y = -look.y * AltYSensitivity * multiplier;
                transform.position += cam.transform.TransformDirection(v);
            }

            // Keyboard movement (speed in metres per second).
            if (move != Vector2.zero)
            {
                var v = Vector3.zero;
                v.x = move.x * MoveXSensitivity * multiplier * Time.deltaTime;
                v.z = move.y * MoveYSensitivity * multiplier * Time.deltaTime;
                transform.position += cam.transform.TransformDirection(v);
            }
        }

        void OnDestroy()
        {
            if (inputActions != null) Destroy(inputActions);
        }

        void OnDisable()
        {
            look = Vector2.zero;
            move = Vector2.zero;
        }

        // When the window loses focus (e.g. putting on the headset), stop everything.
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                look = Vector2.zero;
                move = Vector2.zero;
            }
        }
        #endregion Unity Events

        #region own events
        // Kept for PlayerInput "Send Messages", but ignored on purpose:
        // PlayerInput also listens to the VR controllers. Update reads the keyboard/mouse-only copy.
        public void OnLook(InputValue value) { }
        public void OnMove(InputValue value) { }
        #endregion
    }
}