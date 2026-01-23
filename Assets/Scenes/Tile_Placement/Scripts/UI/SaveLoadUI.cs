using System.IO;
using HexGrid.IO;
using HexGrid.Models;
using HexGrid.Persistence;
using HexGrid.Systems;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using HexGrid.IO; // EditorFileDialogService is in this namespace
#endif

namespace HexGrid.UI
{
    /// Wires UI Toolkit buttons to Save/Load; blocks placement when hovering UI.
    [RequireComponent(typeof(UIDocument))]
    public class SaveLoadUI : MonoBehaviour
    {
        [SerializeField] private string saveButtonName = "Save";
        [SerializeField] private string loadButtonName = "Load";
        [SerializeField] private HexPlacementSystem placementSystem;
        [SerializeField] private CameraController cameraController;

        private Button _save, _load;
        private IMapSerializer _serializer;
        private IFileDialogService _dialog;

        private void Awake()
        {
            _serializer = new JsonMapSerializer();

#if UNITY_EDITOR
            _dialog = new EditorFileDialogService();
#elif USE_SFB
            _dialog = new SFBFileDialogService();
#else
            _dialog = new FallbackFileDialogService();
#endif
        }

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>();
            var root = doc != null ? doc.rootVisualElement : null;

            _save = root?.Q<Button>(saveButtonName);
            _load = root?.Q<Button>(loadButtonName);

            if (_save != null) _save.clicked += OnSaveClicked; else Debug.LogWarning($"Button '{saveButtonName}' not found.");
            if (_load != null) _load.clicked += OnLoadClicked; else Debug.LogWarning($"Button '{loadButtonName}' not found.");

            if (placementSystem != null)
                placementSystem.ShouldBlockInput = UIToolkitHoverBlock.IsPointerOverAnyButton;

            if (cameraController != null)
                cameraController.ShouldBlockInput = UIToolkitHoverBlock.IsPointerOverAnyButton;
        }

        private void OnDisable()
        {
            if (_save != null) _save.clicked -= OnSaveClicked;
            if (_load != null) _load.clicked -= OnLoadClicked;

            if (placementSystem != null)
                placementSystem.ShouldBlockInput = null;

            if (cameraController != null)
                cameraController.ShouldBlockInput = null;
        }

        private void OnSaveClicked()
        {
            if (placementSystem == null) { Debug.LogError("SaveLoadUI: PlacementSystem missing."); return; }

            // Gather tiles from scene markers
            var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var data = new MapDataDTO();
            foreach (var m in markers)
            {
                data.tiles.Add(new PlacedTileDTO
                {
                    prefabIndex = Mathf.Clamp(m.prefabIndex, 0, placementSystem.PrefabCount - 1),
                    x = m.cell.x, y = m.cell.y, z = m.cell.z,
                    yRotation = m.yRotation
                });
            }

            var path = _dialog.SaveFile("Save Map As", "map.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            var json = _serializer.Serialize(data, true);
            File.WriteAllText(path, json);
            Debug.Log($"Saved {data.tiles.Count} tiles to {path}");
        }

        private void OnLoadClicked()
        {
            if (placementSystem == null) { Debug.LogError("SaveLoadUI: PlacementSystem missing."); return; }
            if (!placementSystem.HasGrid) { Debug.LogError("SaveLoadUI: Grid not set on PlacementSystem."); return; }
            if (placementSystem.PrefabCount <= 0) { Debug.LogError("SaveLoadUI: No prefabs set on PlacementSystem."); return; }

            var path = _dialog.OpenFile("Open Map", "json");
            if (string.IsNullOrEmpty(path)) return;

            var json = File.ReadAllText(path);
            var data = _serializer.Deserialize(json);
            if (data == null || data.tiles == null) { Debug.LogWarning("Load failed: invalid JSON."); return; }

            placementSystem.ClearAll();
            placementSystem.RebuildFrom(data);
            Debug.Log($"Loaded {data.tiles.Count} tiles from {path}");
        }
    }
}
