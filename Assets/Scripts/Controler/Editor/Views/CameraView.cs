using Controler.Editor.ViewModels;
using Domain;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Orbit camera View. Reads input and writes to CameraState (Domain),
    /// then applies the smoothed result to the Unity Transform each frame.
    /// RMB = orbit  |  MMB = pan  |  Scroll = zoom  |  F = reset
    /// </summary>
    public class CameraView : MonoBehaviour
    {
        private CameraState _state;
        private Camera      _cam;

        // ── Smoothed current values (Unity-side only) ─────────────────────────
        private float   _yaw, _pitch, _distance;
        private Vector3 _pivot;

        // ── Drag tracking ─────────────────────────────────────────────────────
        private Vector2 _lastMousePos;
        private bool    _orbitBlocked;
        private bool    _panBlocked;

        private void Start()
        {
            var vm = Context.GetApplicationContext()
                            .GetContainer()
                            .Resolve<MapEditorViewModel>();

            _state = vm?.Camera?.Model;
            _cam   = Camera.main;

            if (_state == null)
            {
                Debug.LogError("[CameraView] CameraState not found. Ensure MapEditorBootstrapper runs first.");
                enabled = false;
                return;
            }

            // Snap smoothed values to starting state
            _yaw      = _state.Yaw;
            _pitch    = _state.Pitch;
            _distance = _state.Distance;
            _pivot    = new Vector3(_state.PivotX, _state.PivotY, _state.PivotZ);
            ApplyTransform();
        }

        private void LateUpdate()
        {
            HandleOrbit();
            HandlePan();
            HandleZoom();
            HandleReset();
            SmoothAndApply();
        }

        // ── Input handlers ────────────────────────────────────────────────────

        private void HandleOrbit()
        {
            if (Input.GetMouseButtonDown(1))
            {
                _lastMousePos = Input.mousePosition;
                _orbitBlocked = IsPointerOverUI();
            }
            if (!Input.GetMouseButton(1) || _orbitBlocked) return;

            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            var s = _state.Settings;
            _state.Yaw    += delta.x * s.OrbitSensitivity;
            _state.Pitch  -= delta.y * s.OrbitSensitivity;
            _state.Pitch   = Mathf.Clamp(_state.Pitch, s.MinPitch, s.MaxPitch);
        }

        private void HandlePan()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _lastMousePos = Input.mousePosition;
                _panBlocked   = IsPointerOverUI();
            }
            if (!Input.GetMouseButton(2) || _panBlocked) return;

            Vector2 delta = (Vector2)Input.mousePosition - _lastMousePos;
            _lastMousePos = Input.mousePosition;

            float   scale   = _state.Distance * _state.Settings.PanSensitivity;
            Vector3 right   = new Vector3(transform.right.x,  0f, transform.right.z).normalized;
            Vector3 forward = new Vector3(transform.up.x,     0f, transform.up.z).normalized;

            var pan = -(right * (delta.x * scale)) - (forward * (delta.y * scale));
            _state.PivotX += pan.x;
            _state.PivotY += pan.y;
            _state.PivotZ += pan.z;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(scroll, 0f)) return;

            var s = _state.Settings;

            // Zoom-to-cursor: nudge pivot toward world point under cursor on zoom-in
            if (scroll > 0f)
            {
                var cursor = GetWorldPointUnderCursor();
                _state.PivotX += (cursor.x - _state.PivotX) * 0.12f;
                _state.PivotY += (cursor.y - _state.PivotY) * 0.12f;
                _state.PivotZ += (cursor.z - _state.PivotZ) * 0.12f;
            }

            _state.Distance -= scroll * s.ZoomSpeed * _state.Distance * 0.2f;
            _state.Distance  = Mathf.Clamp(_state.Distance, s.MinZoomDistance, s.MaxZoomDistance);
        }

        private void HandleReset()
        {
            if (Input.GetKeyDown(KeyCode.F))
                _state.Reset();
        }

        // ── Smooth & apply ────────────────────────────────────────────────────

        private void SmoothAndApply()
        {
            var s  = _state.Settings;
            float dt = Time.unscaledDeltaTime;

            _yaw      = Mathf.LerpAngle(_yaw,      _state.Yaw,      s.OrbitSmoothing * dt);
            _pitch    = Mathf.LerpAngle(_pitch,    _state.Pitch,    s.OrbitSmoothing * dt);
            _distance = Mathf.Lerp(_distance,      _state.Distance, s.ZoomSmoothing  * dt);
            _pivot    = Vector3.Lerp(_pivot, new Vector3(_state.PivotX, _state.PivotY, _state.PivotZ),
                                    s.PanSmoothing * dt);
            ApplyTransform();
        }

        private void ApplyTransform()
        {
            Quaternion rot     = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _pivot + rot * new Vector3(0f, 0f, -_distance);
            transform.rotation = rot;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private Vector3 GetWorldPointUnderCursor()
        {
            Ray   ray    = _cam.ScreenPointToRay(Input.mousePosition);
            float planeY = _state.PivotY;
            if (Mathf.Abs(ray.direction.y) > 0.001f)
            {
                float t = (planeY - ray.origin.y) / ray.direction.y;
                if (t > 0f) return ray.GetPoint(t);
            }
            return ray.GetPoint(_state.Distance);
        }

        private static bool IsPointerOverUI() =>
            EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
