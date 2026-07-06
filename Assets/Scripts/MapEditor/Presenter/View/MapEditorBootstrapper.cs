using AssetImporter.App.UseCase;
using Campaign.App.Port;
using Campaign.App.UseCase;
using Campaign.Infrastructure;
using Camera.Domain;
using Grid.Domain;
using MapEditor.App.UseCase;
using MapEditor.Domain;
using MapEditor.Presenter.ViewModels;
using SceneEditor.App.UseCase;
using Shared.App.Port;
using Shared.Domain;
using Shared.Infrastructure;
using UnityEngine;
using DomainGrid = Grid.Domain.Grid;
using Loxodon.Framework.Localizations;

namespace MapEditor.Presenter.View
{
    public class MapEditorBootstrapper : MonoBehaviour
    {
        [Header("Initial map settings")]
        [SerializeField] private string mapName      = "Default Map";
        [SerializeField] private float  cellSize     = 1f;
        [SerializeField] private int    mapWidth     = 20;
        [SerializeField] private int    mapDepth     = 20;
        [SerializeField] private int    mapThickness = 3;

        private MapEditorViewModel _vm;

        private void OnDestroy()
        {
            _vm?.Dispose();
        }

        private void Awake()
        {
            // ── Domain ────────────────────────────────────────────────────────
            var grid = new DomainGrid { CellSize = cellSize, SurfaceY = 0f };

            var terrain = new TerrainLayout
            {
                Width     = mapWidth,
                Depth     = mapDepth,
                Thickness = mapThickness,
                Height    = 0f,
                Color     = new[] { 0.6f, 0.4f, 0.2f, 1f },
                Grid      = grid
            };

            var map = new Map
            {
                Id            = System.Guid.NewGuid().ToString(),
                Name          = mapName,
                TerrainLayout = terrain
            };

            var history     = new CommandHistory();
            var cameraState = new CameraState
            {
                InitialPivotX    = 10f,
                InitialPivotY    = 0f,
                InitialPivotZ    = -5f,
                InitialYaw      = 50f,
                InitialPitch    = 50f,
                InitialDistance = 20f
            };

            // Share the same CameraSettings instance so live slider edits are
            // automatically reflected on the Map for saving, with no extra sync code.
            map.CameraSettings = cameraState.Settings;

            // ── Infrastructure ────────────────────────────────────────────────
            IMapSerializer     serializer = new JsonMapSerializer();
            IFileDialogService dialog     = new FileDialogService();

            // ── Session & permissions ────────────────────────────────────────
            ISessionContext    session     = new LocalSessionContext(map);
            IPermissionService permissions = new PermissionService();

            // ── Use cases ─────────────────────────────────────────────────────
            var placeObject     = new PlaceObjectUseCase(map, grid, history, session);
            var deleteObject    = new DeleteObjectUseCase(map, grid, history, session, permissions);
            var transformObject = new TransformObjectUseCase(history, session, permissions);
            var setObjectMetadata = new SetObjectMetadataUseCase(history);
            var removeObjectMetadata = new RemoveObjectMetadataUseCase(history);
            var createTag       = new CreateTagUseCase(history, map);
            var renameTag       = new RenameTagUseCase(history, map);
            var deleteTag       = new DeleteTagUseCase(history, map);
            var createPlayer    = new CreatePlayerUseCase(history, map);
            var deletePlayer    = new DeletePlayerUseCase(history, map, session);
            var generateTerrain = new GenerateTerrainUseCase(history, grid, map);
            var saveMap         = new SaveMapUseCase(serializer, dialog);
            var loadMap         = new LoadMapUseCase(serializer, dialog);
            var importAsset     = new ImportAssetUseCase(dialog);

            // ── ViewModel ─────────────────────────────────────────────────────
            _vm = new MapEditorViewModel(
                map, cameraState, history, session, permissions,
                placeObject, deleteObject, transformObject, setObjectMetadata, removeObjectMetadata,
                createTag, renameTag, deleteTag, createPlayer, deletePlayer, generateTerrain,
                saveMap, loadMap, importAsset);

            _vm.Register();
        }
    }
}
