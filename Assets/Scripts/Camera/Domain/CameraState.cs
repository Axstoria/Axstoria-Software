namespace Domain
{
    /// <summary>
    /// Tunable settings for the editor camera.
    /// </summary>
    public class CameraSettings
    {
        // Orbit
        public float OrbitSensitivity = 0.4f;
        public float MinPitch         = 5f;
        public float MaxPitch         = 85f;
        public float OrbitSmoothing   = 16f;

        // Zoom
        public float ZoomSpeed       = 4f;
        public float ZoomSmoothing   = 10f;
        public float MinZoomDistance = 2f;
        public float MaxZoomDistance = 80f;

        // Pan
        public float PanSensitivity = 0.04f;
        public float PanSmoothing   = 16f;
    }

    /// <summary>
    /// Camere state current target values and initial values for reset.
    /// </summary>
    public class CameraState
    {
        public CameraSettings Settings { get; } = new();

        // ── Target state (what input writes to) ───────────────────────────────
        public float PivotX   { get; set; }
        public float PivotY   { get; set; }
        public float PivotZ   { get; set; }
        public float Yaw      { get; set; }
        public float Pitch    { get; set; }
        public float Distance { get; set; }

        // ── Initial / reset values ────────────────────────────────────────────
        public float InitialPivotX    { get; set; }
        public float InitialPivotY    { get; set; }
        public float InitialPivotZ    { get; set; }
        public float InitialYaw      { get; set; } = 45f;
        public float InitialPitch    { get; set; } = 50f;
        public float InitialDistance { get; set; } = 20f;

        public CameraState()
        {
            Reset();
        }

        /// <summary>Snaps all target values back to their initial state.</summary>
        public void Reset()
        {
            PivotX   = InitialPivotX;
            PivotY   = InitialPivotY;
            PivotZ   = InitialPivotZ;
            Yaw      = InitialYaw;
            Pitch    = InitialPitch;
            Distance = InitialDistance;
        }

        /// <summary>Moves the pivot target to focus on a world point.</summary>
        public void FocusOn(float x, float y, float z, float distance = -1f)
        {
            PivotX = x;
            PivotY = y;
            PivotZ = z;
            if (distance > 0f) Distance = distance;
        }
    }
}
