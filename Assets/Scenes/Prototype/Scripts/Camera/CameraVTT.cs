using UnityEngine;
// TODO: replace hardcoded keys with a configurable InputAction asset.

namespace VTT
{
    /// <summary>
    /// Orbit camera.
    /// RMB = orbit  |  MMB = pan  |  Scroll = zoom  |  F = reset
    /// </summary>
    public class CameraVTT : MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField] private float orbitSensitivity = 0.4f;
        [SerializeField] private float minPolarAngle    = 5f;
        [SerializeField] private float maxPolarAngle    = 85f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed        = 4f;
        [SerializeField] private float zoomSmoothing    = 10f;
        [SerializeField] private float minZoomDistance  = 2f;
        [SerializeField] private float maxZoomDistance  = 80f;

        [Header("Pan")]
        [SerializeField] private float panSensitivity   = 0.04f;

        [Header("Smoothing")]
        [SerializeField] private float orbitSmoothing   = 16f;
        [SerializeField] private float panSmoothing     = 16f;

        [Header("Starting Transform")]
        [SerializeField] private Vector3 initialPivot    = Vector3.zero;
        [SerializeField] private float   initialDistance = 20f;
        [SerializeField] private float   initialYaw      = 45f;
        [SerializeField] private float   initialPitch    = 50f;

        private Vector3 _pivot, _targetPivot;
        private float   _yaw, _targetYaw;
        private float   _pitch, _targetPitch;
        private float   _distance, _targetDistance;
        private Vector2 _lastMousePos;

        // Track whether each drag started over the UI so mid-drag UI transitions don't interrupt it.
        private bool _orbitBlocked;
        private bool _panBlocked;

        private void Awake()
        {
            _pivot    = _targetPivot    = initialPivot;
            _yaw      = _targetYaw      = initialYaw;
            _pitch    = _targetPitch    = initialPitch;
            _distance = _targetDistance = initialDistance;
            ApplyTransform(snap: true);
        }

        private void LateUpdate()
        {
            HandleOrbit();
            HandlePan();
            HandleZoom();
            HandleFrameReset();
            SmoothAndApply();
        }

        private void HandleOrbit()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _lastMousePos  = Input.mousePosition;
                _orbitBlocked  = false;
            }
            if (!Input.GetMouseButton(1) || _orbitBlocked) return;

            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            _targetYaw   += delta.x * orbitSensitivity;
            _targetPitch -= delta.y * orbitSensitivity;
            _targetPitch  = Mathf.Clamp(_targetPitch, minPolarAngle, maxPolarAngle);
        }

        private void HandlePan()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _lastMousePos = Input.mousePosition;
                _panBlocked   = false;
            }
            if (!Input.GetMouseButton(2) || _panBlocked) return;

            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            float   scale    = _distance * panSensitivity;
            Vector3 right    = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
            Vector3 forward  = new Vector3(transform.up.x,    0f, transform.up.z).normalized;

            _targetPivot -= right   * (delta.x * scale);
            _targetPivot -= forward * (delta.y * scale);
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f)) return;

            // Shift pivot slightly toward cursor when zooming in for a "zoom-to-cursor" feel.
            if (scroll > 0f)
                _targetPivot += (GetWorldPointUnderCursor() - _targetPivot) * 0.12f;

            _targetDistance -= scroll * zoomSpeed * _targetDistance * 0.2f;
            _targetDistance  = Mathf.Clamp(_targetDistance, minZoomDistance, maxZoomDistance);
        }

        private void HandleFrameReset()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                _targetPivot    = initialPivot;
                _targetDistance = initialDistance;
                _targetYaw      = initialYaw;
                _targetPitch    = initialPitch;
            }
        }

        private void SmoothAndApply()
        {
            float dt  = Time.unscaledDeltaTime;
            _yaw      = Mathf.LerpAngle(_yaw,      _targetYaw,      orbitSmoothing * dt);
            _pitch    = Mathf.LerpAngle(_pitch,     _targetPitch,    orbitSmoothing * dt);
            _distance = Mathf.Lerp(_distance,       _targetDistance, zoomSmoothing  * dt);
            _pivot    = Vector3.Lerp(_pivot,         _targetPivot,   panSmoothing   * dt);
            ApplyTransform(snap: false);
        }

        private void ApplyTransform(bool snap)
        {
            if (snap)
            {
                _yaw = _targetYaw; _pitch = _targetPitch;
                _distance = _targetDistance; _pivot = _targetPivot;
            }
            Quaternion rot     = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _pivot + rot * new Vector3(0f, 0f, -_distance);
            transform.rotation = rot;
        }

        /// <summary>
        /// Raycast onto the horizontal plane at pivot height
        /// </summary>
        private Vector3 GetWorldPointUnderCursor()
        {
            Ray   ray    = Camera.main.ScreenPointToRay(Input.mousePosition);
            float planeY = _targetPivot.y;
            if (Mathf.Abs(ray.direction.y) > 0.001f)
            {
                float t = (planeY - ray.origin.y) / ray.direction.y;
                if (t > 0f) return ray.GetPoint(t);
            }
            return ray.GetPoint(_distance);
        }

        public void FocusOn(Vector3 worldPoint, float distance = -1f)
        {
            _targetPivot = worldPoint;
            if (distance > 0f) _targetDistance = distance;
        }

        public Vector3 Pivot => _pivot;
    }
}
