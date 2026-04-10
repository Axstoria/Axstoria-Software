using Domain;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// Exposes CameraState properties as observables so UI panels can
    /// bind to and modify camera settings without touching the View directly.
    /// </summary>
    public class CameraViewModel
    {
        public CameraState Model { get; }

        // ── Observable settings ───────────────────────────────────────────────

        public ObservableProperty<float> OrbitSensitivity { get; } = new();
        public ObservableProperty<float> PanSensitivity   { get; } = new();
        public ObservableProperty<float> ZoomSpeed        { get; } = new();
        public ObservableProperty<float> MinZoomDistance  { get; } = new();
        public ObservableProperty<float> MaxZoomDistance  { get; } = new();
        public ObservableProperty<float> MinPitch         { get; } = new();
        public ObservableProperty<float> MaxPitch         { get; } = new();

        public CameraViewModel(CameraState model)
        {
            Model = model;

            // Seed observables from model
            OrbitSensitivity.Value = model.Settings.OrbitSensitivity;
            PanSensitivity.Value   = model.Settings.PanSensitivity;
            ZoomSpeed.Value        = model.Settings.ZoomSpeed;
            MinZoomDistance.Value  = model.Settings.MinZoomDistance;
            MaxZoomDistance.Value  = model.Settings.MaxZoomDistance;
            MinPitch.Value         = model.Settings.MinPitch;
            MaxPitch.Value         = model.Settings.MaxPitch;

            // Write-back: UI changes propagate to domain
            OrbitSensitivity.ValueChanged += (_, __) => model.Settings.OrbitSensitivity = OrbitSensitivity.Value;
            PanSensitivity.ValueChanged   += (_, __) => model.Settings.PanSensitivity   = PanSensitivity.Value;
            ZoomSpeed.ValueChanged        += (_, __) => model.Settings.ZoomSpeed        = ZoomSpeed.Value;
            MinZoomDistance.ValueChanged  += (_, __) => model.Settings.MinZoomDistance  = MinZoomDistance.Value;
            MaxZoomDistance.ValueChanged  += (_, __) => model.Settings.MaxZoomDistance  = MaxZoomDistance.Value;
            MinPitch.ValueChanged         += (_, __) => model.Settings.MinPitch         = MinPitch.Value;
            MaxPitch.ValueChanged         += (_, __) => model.Settings.MaxPitch         = MaxPitch.Value;
        }

        public void Reset() => Model.Reset();
    }
}
