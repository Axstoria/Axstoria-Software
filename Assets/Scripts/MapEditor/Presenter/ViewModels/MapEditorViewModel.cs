using AssetImporter.App.UseCase;
using Campaign.App.UseCase;
using Camera.Domain;
using Camera.Presenter.ViewModels;
using DomainGrid = Grid.Domain.Grid;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Observables;
using MapEditor.App.UseCase;
using MapEditor.Domain;
using SceneEditor.App.UseCase;
using Shared.Domain;

namespace MapEditor.Presenter.ViewModels
{
    public class MapEditorViewModel : ObservableObject
    {
        private readonly CommandHistory _history;

        // ── Sub-ViewModels ────────────────────────────────────────────────────
        public MapViewModel    Map    { get; }
        public CameraViewModel Camera { get; }
        public DomainGrid      Grid   => Map.Terrain?.Model?.Grid;

        // ── Observable state ──────────────────────────────────────────────────
        public ObservableProperty<bool>   CanUndo         { get; } = new();
        public ObservableProperty<bool>   CanRedo         { get; } = new();
        public ObservableProperty<string> UndoLabel       { get; } = new();
        public ObservableProperty<bool>   IsPlacementMode { get; } = new();
        public ObservableProperty<string> Status          { get; } = new();
        public ObservableProperty<bool>   IsBusy          { get; } = new();

        // ── Use cases ─────────────────────────────────────────────────────────
        public PlaceObjectUseCase     PlaceObject     { get; }
        public DeleteObjectUseCase    DeleteObject    { get; }
        public TransformObjectUseCase TransformObject { get; }
        public GenerateTerrainUseCase GenerateTerrain { get; }
        public SaveMapUseCase         SaveMap         { get; }
        public LoadMapUseCase         LoadMap         { get; }
        public ImportAssetUseCase     ImportAsset     { get; }

        public MapEditorViewModel(
            Map                   map,
            CameraState           cameraState,
            CommandHistory        history,
            PlaceObjectUseCase    placeObject,
            DeleteObjectUseCase   deleteObject,
            TransformObjectUseCase transformObject,
            GenerateTerrainUseCase generateTerrain,
            SaveMapUseCase        saveMap,
            LoadMapUseCase        loadMap,
            ImportAssetUseCase    importAsset)
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

        public void Undo() => _history.Undo();
        public void Redo() => _history.Redo();

        public void Dispose()
        {
            _history.OnHistoryChanged -= SyncHistoryState;
            Map.Dispose();
        }

        public void Register()
        {
            Context.GetApplicationContext()
                   .GetContainer()
                   .Register<MapEditorViewModel>(this);
        }

        private void SyncHistoryState()
        {
            CanUndo.Value   = _history.CanUndo;
            CanRedo.Value   = _history.CanRedo;
            UndoLabel.Value = _history.UndoLabel;
        }
    }
}
