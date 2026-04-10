
using App.Ports;
using App.UseCases;
using Controler.Editor.ViewModels;
using Domain;
using DomainGrid = Domain.Grid;
using Infrastructure.IO;
using Infrastructure.Persistence;
using UnityEngine;

namespace Controler.Editor.Views
{
    /// <summary>
    /// Composition root for the map editor scene.
    /// Creates and wires all Domain, Infrastructure, Application, and ViewModel objects,
    /// then registers the MapEditorViewModel in the Loxodon context.
    /// Must execute before all other Views — keep Script Execution Order above default.
    /// </summary>
    public class MapEditorBootstrapper : MonoBehaviour
    {
        [Header("Initial map settings")]
        [SerializeField] private string mapName    = "Default Map";
        [SerializeField] private float  cellSize   = 1f;
        [SerializeField] private int    mapWidth   = 20;
        [SerializeField] private int    mapDepth   = 20;
        [SerializeField] private int    mapThickness = 3;

        private MapEditorViewModel _vm; // not readonly — assigned in Awake, not constructor

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

            // ── Infrastructure ────────────────────────────────────────────────
            IMapSerializer     serializer = new JsonMapSerializer();
            IFileDialogService dialog     = new FileDialogService();

            // ── Use cases ─────────────────────────────────────────────────────
            var placeObject     = new PlaceObjectUseCase(map, grid, history);
            var deleteObject    = new DeleteObjectUseCase(map, grid, history);
            var transformObject = new TransformObjectUseCase(history);
            var generateTerrain = new GenerateTerrainUseCase(history, grid, map);
            var saveMap         = new SaveMapUseCase(serializer, dialog);
            var loadMap         = new LoadMapUseCase(serializer, dialog);
            var importAsset     = new ImportAssetUseCase(map, history, dialog);

            // ── ViewModel ─────────────────────────────────────────────────────
            _vm = new MapEditorViewModel(
                map, cameraState, history,
                placeObject, deleteObject, transformObject, generateTerrain,
                saveMap, loadMap, importAsset);

            _vm.Register();
        }
    }
}
