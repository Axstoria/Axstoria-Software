using UnityEngine;

namespace HexGrid.Models
{
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "HexGrid/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Zoom")]
        [Tooltip("Minimum distance from the pivot point")]
        public float zoomMin = 5f;
        [Tooltip("Maximum distance from the pivot point")]
        public float zoomMax = 50f;
        [Tooltip("Zoom speed multiplier for scroll input")]
        public float zoomSpeed = 15f;
        [Tooltip("Smoothing factor for zoom interpolation (higher = faster)")]
        public float zoomSmoothing = 10f;

        [Header("Pan")]
        [Tooltip("Mouse pan speed multiplier")]
        public float panSpeedMouse = 0.15f;
        [Tooltip("Keyboard pan speed multiplier")]
        public float panSpeedKeyboard = 20f;
        [Tooltip("Smoothing factor for pan interpolation (higher = faster)")]
        public float panSmoothing = 10f;

        [Header("Orbit")]
        [Tooltip("Rotation speed multiplier for mouse drag")]
        public float orbitSpeed = 0.3f;
        [Tooltip("Minimum pitch angle (looking down)")]
        public float pitchMin = 10f;
        [Tooltip("Maximum pitch angle (looking down)")]
        public float pitchMax = 89f;
        [Tooltip("Smoothing factor for orbit interpolation (higher = faster)")]
        public float orbitSmoothing = 10f;

        [Header("Default View")]
        [Tooltip("Default camera pitch angle")]
        public float defaultPitch = 45f;
        [Tooltip("Default camera yaw angle")]
        public float defaultYaw = 0f;
        [Tooltip("Default zoom distance")]
        public float defaultZoom = 20f;
    }
}
