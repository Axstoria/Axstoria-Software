using System;
using HexGrid.Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HexGrid.Systems
{
    /// Main controller for editor camera: coordinates zoom, pan and orbit managers.
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CameraSettings settings;
        [SerializeField] private Transform cameraTransform;

        /// External callback to block input (e.g., when hovering UI).
        public Func<bool> ShouldBlockInput;

        private CameraZoomManager _zoomManager;
        private CameraMovementManager _movementManager;
        private CameraOrbitManager _orbitManager;

        private void Awake()
        {
            if (cameraTransform == null)
            {
                cameraTransform = Camera.main?.transform;
            }

            if (settings == null)
            {
                Debug.LogError("CameraController: CameraSettings not assigned.");
                enabled = false;
                return;
            }

            float initialDistance = settings.defaultZoom;
            Vector3 initialPivot = Vector3.zero;

            _zoomManager = new CameraZoomManager(settings, initialDistance);
            _movementManager = new CameraMovementManager(settings, initialPivot);
            _orbitManager = new CameraOrbitManager(settings, settings.defaultPitch, settings.defaultYaw);

            ApplyCameraTransform();
        }

        private void Update()
        {
            if (cameraTransform == null || settings == null) return;

            bool inputBlocked = ShouldBlockInput != null && ShouldBlockInput();

            if (!inputBlocked)
            {
                HandleZoomInput();
                HandlePanInput();
                HandleOrbitInput();
                HandleKeyboardPan();
            }

            _zoomManager.UpdateZoom(Time.deltaTime);
            _movementManager.UpdateMovement(Time.deltaTime);
            _orbitManager.UpdateRotation(Time.deltaTime);

            ApplyCameraTransform();
        }

        private void HandleZoomInput()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (!Mathf.Approximately(scroll, 0f))
            {
                _zoomManager.ProcessZoomInput(scroll * 0.01f);
            }
        }

        /// Pan: middle mouse or Shift+left click (trackpad alternative).
        private void HandlePanInput()
        {
            bool shiftPressed = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            bool panStarted = Mouse.current.middleButton.wasPressedThisFrame ||
                              (shiftPressed && Mouse.current.leftButton.wasPressedThisFrame);

            if (panStarted && !_orbitManager.IsOrbiting)
            {
                _movementManager.StartPan(Mouse.current.position.ReadValue());
            }

            if (_movementManager.IsPanning)
            {
                bool panHeld = Mouse.current.middleButton.isPressed ||
                               (shiftPressed && Mouse.current.leftButton.isPressed);

                if (panHeld)
                {
                    _movementManager.UpdatePan(
                        Mouse.current.position.ReadValue(),
                        cameraTransform.right,
                        cameraTransform.forward
                    );
                }
                else
                {
                    _movementManager.StopPan();
                }
            }
        }

        /// Orbit: Alt+left click to rotate camera around pivot.
        private void HandleOrbitInput()
        {
            bool altPressed = Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed;

            if (altPressed && Mouse.current.leftButton.wasPressedThisFrame && !_movementManager.IsPanning)
            {
                _orbitManager.StartOrbit(Mouse.current.position.ReadValue());
            }

            if (_orbitManager.IsOrbiting)
            {
                if (altPressed && Mouse.current.leftButton.isPressed)
                {
                    _orbitManager.UpdateOrbit(Mouse.current.position.ReadValue());
                }
                else
                {
                    _orbitManager.StopOrbit();
                }
            }
        }

        /// Keyboard pan: WASD/ZQSD (supports both QWERTY and AZERTY).
        private void HandleKeyboardPan()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.zKey.isPressed)
                input.y += 1f;
            if (Keyboard.current.sKey.isPressed)
                input.y -= 1f;
            if (Keyboard.current.dKey.isPressed)
                input.x += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.qKey.isPressed)
                input.x -= 1f;

            if (input.sqrMagnitude > 0.01f)
            {
                input.Normalize();
                _movementManager.ProcessKeyboardInput(
                    input,
                    cameraTransform.right,
                    cameraTransform.forward,
                    Time.deltaTime
                );
            }
        }

        /// Positions camera at distance from pivot, looking at pivot point.
        private void ApplyCameraTransform()
        {
            Quaternion rotation = _orbitManager.GetRotation();
            Vector3 offset = rotation * Vector3.back * _zoomManager.CurrentDistance;
            Vector3 pivot = _movementManager.PivotPosition;

            cameraTransform.position = pivot + offset;
            cameraTransform.rotation = rotation;
        }

        public void FocusOnPosition(Vector3 position)
        {
            _movementManager.FocusOnPosition(position);
        }

        public void ResetView()
        {
            _movementManager.SetPivotPosition(Vector3.zero);
            _zoomManager.SetDistance(settings.defaultZoom);
            _orbitManager.SetRotation(settings.defaultPitch, settings.defaultYaw);
        }
    }
}
