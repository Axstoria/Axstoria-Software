using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Systems
{
    /// Manages camera zoom with smooth interpolation between current and target distance.
    public class CameraZoomManager
    {
        private readonly CameraSettings _settings;

        private float _currentDistance;
        /// Target distance for smooth interpolation.
        private float _targetDistance;
        private float _zoomVelocity;

        public float CurrentDistance => _currentDistance;
        public float TargetDistance => _targetDistance;

        public CameraZoomManager(CameraSettings settings, float initialDistance)
        {
            _settings = settings;
            _currentDistance = initialDistance;
            _targetDistance = initialDistance;
        }

        public void ProcessZoomInput(float scrollDelta)
        {
            if (Mathf.Approximately(scrollDelta, 0f)) return;

            _targetDistance -= scrollDelta * _settings.zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, _settings.zoomMin, _settings.zoomMax);
        }

        public float UpdateZoom(float deltaTime)
        {
            _currentDistance = Mathf.SmoothDamp(
                _currentDistance,
                _targetDistance,
                ref _zoomVelocity,
                1f / _settings.zoomSmoothing
            );
            return _currentDistance;
        }

        public void SetDistance(float distance)
        {
            _currentDistance = Mathf.Clamp(distance, _settings.zoomMin, _settings.zoomMax);
            _targetDistance = _currentDistance;
            _zoomVelocity = 0f;
        }
    }
}
