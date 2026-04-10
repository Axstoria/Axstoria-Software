using App.UseCases;
using Domain;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Observables;

namespace Controler.Editor.ViewModels
{
    /// <summary>
    /// Central ViewModel for the map editor scene.
    /// Registered in the Loxodon context by the Bootstrapper so all Views can resolve it.
    /// Owns all use cases and exposes the state the UI needs to observe.
    /// </summary>
    public class MapEditorViewModel : ObservableObject
    {
        private readonly CommandHistory _history;

        // ── Sub-ViewModels ────────────────────────────────────────────────────

        public MapViewModel    Map    { get; }
        public CameraViewModel Camera { get; }
        public Grid            Grid   => Map.Terrain?.Model?.Grid;

        // ── Observable state ──────────────────────────────────────────────────

        public ObservableProperty<bool>   CanUndo        { get; } = new();
        public ObservableProperty<bool>   CanRedo        { get; } = new();
        public ObservableProperty<string> UndoLabel      { get; } = new();
        public ObservableProperty<bool>   IsPlacementMode { get; } = new();
        public ObservableProperty<string> Status         { get; } = new();
        public ObservableProperty<bool>   IsBusy         { get; } = new();

        // ── Use cases (called by Views) ───────────────────────────────────────

        public PlaceObjectUseCase    PlaceObject     { get; }
        public DeleteObjectUseCase   DeleteObject    { get; }
        public TransformObjectUseCase TransformObject { get; }
        public GenerateTerrainUseCase GenerateTerrain { get; }
        public SaveMapUseCase        SaveMap          { get; }
        public LoadMapUseCase        LoadMap          { get; }
        public ImportAssetUseCase    ImportAsset      { get; }

        // ── Constructor ───────────────────────────────────────────────────────

        public MapEditorViewModel(
            Domain.Map             map,
            CameraState            cameraState,
            CommandHistory         history,
            PlaceObjectUseCase     placeObject,
            DeleteObjectUseCase    deleteObject,
            TransformObjectUseCase transformObject,
            GenerateTerrainUseCase generateTerrain,
            SaveMapUseCase         saveMap,
            LoadMapUseCase         loadMap,
            ImportAssetUseCase     importAsset)
        {
            _history = history;

            Map             = new MapViewModel(map);
            Camera          = new CameraViewModel(cameraState);
            PlaceObject     = placeObject;
            DeleteObject    = deleteObject;
            TransformObject = transformObject;
            GenerateTerrain = generateTerrain;
            SaveMap         = saveMap;
            LoadMap         = loadMap;
            ImportAsset     = importAsset;

            _history.OnHistoryChanged += SyncHistoryState;
            SyncHistoryState();
        }

        // ── History passthrough ───────────────────────────────────────────────

        public void Undo() => _history.Undo();
        public void Redo() => _history.Redo();

        public void Dispose()
        {
            _history.OnHistoryChanged -= SyncHistoryState;
            Map.Dispose();
        }

        // ── Registration ──────────────────────────────────────────────────────

        public void Register()
        {
            Context.GetApplicationContext()
                   .GetContainer()
                   .Register<MapEditorViewModel>(this);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void SyncHistoryState()
        {
            CanUndo.Value   = _history.CanUndo;
            CanRedo.Value   = _history.CanRedo;
            UndoLabel.Value = _history.UndoLabel;
        }
    }
}
