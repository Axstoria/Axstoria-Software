using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages camera pan movement around a pivot point on the XZ plane.
    public class CameraMovementManager
    {
        private readonly CameraSettings _settings;

        /// The point the camera orbits around and looks at.
        private Vector3 _pivotPosition;
        private Vector3 _targetPivotPosition;
        private Vector3 _panVelocity;

        private bool _isPanning;
        private Vector2 _panStartMousePos;
        private Vector3 _panStartPivotPos;

        public Vector3 PivotPosition => _pivotPosition;
        public bool IsPanning => _isPanning;

        public CameraMovementManager(CameraSettings settings, Vector3 initialPivot)
        {
            _settings = settings;
            _pivotPosition = initialPivot;
            _targetPivotPosition = initialPivot;
        }

        public void StartPan(Vector2 mousePosition)
        {
            _isPanning = true;
            _panStartMousePos = mousePosition;
            _panStartPivotPos = _targetPivotPosition;
        }

        public void UpdatePan(Vector2 currentMousePosition, Vector3 cameraRight, Vector3 cameraForward)
        {
            if (!_isPanning) return;

            Vector2 mouseDelta = currentMousePosition - _panStartMousePos;

            // Project camera directions onto XZ plane for horizontal movement
            Vector3 flatRight = new Vector3(cameraRight.x, 0f, cameraRight.z).normalized;
            Vector3 flatForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;

            Vector3 movement = (-flatRight * mouseDelta.x - flatForward * mouseDelta.y) * _settings.panSpeedMouse;
            _targetPivotPosition = _panStartPivotPos + movement;
        }

        public void StopPan()
        {
            _isPanning = false;
        }

        public void ProcessKeyboardInput(Vector2 inputAxis, Vector3 cameraRight, Vector3 cameraForward, float deltaTime)
        {
            if (inputAxis.sqrMagnitude < 0.01f) return;

            Vector3 flatRight = new Vector3(cameraRight.x, 0f, cameraRight.z).normalized;
            Vector3 flatForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;

            Vector3 movement = (flatRight * inputAxis.x + flatForward * inputAxis.y) * _settings.panSpeedKeyboard * deltaTime;
            _targetPivotPosition += movement;
        }

        public Vector3 UpdateMovement(float deltaTime)
        {
            _pivotPosition = Vector3.SmoothDamp(
                _pivotPosition,
                _targetPivotPosition,
                ref _panVelocity,
                1f / _settings.panSmoothing
            );
            return _pivotPosition;
        }

        public void SetPivotPosition(Vector3 position)
        {
            _pivotPosition = position;
            _targetPivotPosition = position;
            _panVelocity = Vector3.zero;
        }

        public void FocusOnPosition(Vector3 position)
        {
            _targetPivotPosition = position;
        }
    }
}
