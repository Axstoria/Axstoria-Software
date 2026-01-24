using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages camera orbit rotation (pitch = vertical angle, yaw = horizontal angle).
    public class CameraOrbitManager
    {
        private readonly CameraSettings _settings;

        /// Vertical angle (looking up/down). Clamped to avoid camera flip.
        private float _currentPitch;
        /// Horizontal angle (looking left/right).
        private float _currentYaw;
        private float _targetPitch;
        private float _targetYaw;
        private float _pitchVelocity;
        private float _yawVelocity;

        private bool _isOrbiting;
        private Vector2 _orbitStartMousePos;
        private float _orbitStartPitch;
        private float _orbitStartYaw;

        public float CurrentPitch => _currentPitch;
        public float CurrentYaw => _currentYaw;
        public bool IsOrbiting => _isOrbiting;

        public CameraOrbitManager(CameraSettings settings, float initialPitch, float initialYaw)
        {
            _settings = settings;
            _currentPitch = initialPitch;
            _currentYaw = initialYaw;
            _targetPitch = initialPitch;
            _targetYaw = initialYaw;
        }

        public void StartOrbit(Vector2 mousePosition)
        {
            _isOrbiting = true;
            _orbitStartMousePos = mousePosition;
            _orbitStartPitch = _targetPitch;
            _orbitStartYaw = _targetYaw;
        }

        public void UpdateOrbit(Vector2 currentMousePosition)
        {
            if (!_isOrbiting) return;

            Vector2 mouseDelta = currentMousePosition - _orbitStartMousePos;

            _targetYaw = _orbitStartYaw + mouseDelta.x * _settings.orbitSpeed;
            _targetPitch = _orbitStartPitch - mouseDelta.y * _settings.orbitSpeed;
            _targetPitch = Mathf.Clamp(_targetPitch, _settings.pitchMin, _settings.pitchMax);
        }

        public void StopOrbit()
        {
            _isOrbiting = false;
        }

        public void UpdateRotation(float deltaTime)
        {
            _currentPitch = Mathf.SmoothDamp(
                _currentPitch,
                _targetPitch,
                ref _pitchVelocity,
                1f / _settings.orbitSmoothing
            );

            _currentYaw = Mathf.SmoothDamp(
                _currentYaw,
                _targetYaw,
                ref _yawVelocity,
                1f / _settings.orbitSmoothing
            );
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(_currentPitch, _currentYaw, 0f);
        }

        public void SetRotation(float pitch, float yaw)
        {
            _currentPitch = Mathf.Clamp(pitch, _settings.pitchMin, _settings.pitchMax);
            _currentYaw = yaw;
            _targetPitch = _currentPitch;
            _targetYaw = _currentYaw;
            _pitchVelocity = 0f;
            _yawVelocity = 0f;
        }
    }
}
