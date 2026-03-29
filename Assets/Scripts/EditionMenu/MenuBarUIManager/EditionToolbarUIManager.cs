using UnityEngine;
using UnityEngine.UIElements;
using Fab.UITKDropdown;
using System.Collections.Generic;
using JetBrains.Annotations;
using Edition.Systems;
using Edition.IO;
using Edition.Models;
using Edition.Persistence;
using System.IO;
// ADDED: bring in the prototype namespaces
using VTT;
using VTT.Persistence;

public class EditionToolbarUIManager : MonoBehaviour, IUIManager
{
    private ThemeStyleSheet theme;
    private VisualElement root;
    private VisualElement managedUI;
    private const string UIName = "Toolbar";
    private HexPlacementSystem placementSystem;

    private Dropdown dropdown;

    private DropdownMenu fileMenu;
    private DropdownMenu editMenu;
    private DropdownMenu viewMenu = new();
    private DropdownMenu toolsMenu;

    private List<IUIManager> toggleableUIs = new List<IUIManager>();
    private IMapSerializer _serializer;
    private IFileDialogService _dialog;
    private GltfImporter _gltfImporter;

    string IUIManager.Name => UIName;

    public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme)
    {
        this.root = root;
        this.managedUI = managedUI;
        this.theme = theme;
        transform.position = Vector3.one;
    }

    public void SetPlacementSystem(HexPlacementSystem system)
    {
        placementSystem = system;
    }

    public void AddToggleableUI(IUIManager element)
    {
        toggleableUIs.Add(element);
    }

    private void Awake()
    {
        _serializer   = new JsonMapSerializer();
        _gltfImporter = new GltfImporter();

#if UNITY_EDITOR
        _dialog = new EditorFileDialogService();
#elif USE_SFB
        _dialog = new SFBFileDialogService();
#else
        _dialog = new FallbackFileDialogService();
#endif
    }

    private void Start()
    {
        dropdown = new Dropdown(root);

        fileMenu = new DropdownMenu();
        fileMenu.AppendAction("Save",              OnSaveClicked);
        fileMenu.AppendAction("Import Map",        OnLoadClicked);
        fileMenu.AppendAction("Import Asset",      OnImportAssetClicked);
        fileMenu.AppendAction("Open/Rules",        null);
        fileMenu.AppendAction("Open/Sheets",       null);
        fileMenu.AppendAction("Link to object/Notes",  null);
        fileMenu.AppendAction("Link to object/Sheets", null);

        root.Q<Button>("file-button").clickable.clickedWithEventInfo +=
            evt => dropdown.Open(fileMenu, evt);

        editMenu = new DropdownMenu();
        // ADDED: Undo() / Redo()
        editMenu.AppendAction("Undo action", OnUndoClicked);
        editMenu.AppendAction("Redo action", OnRedoClicked);
        editMenu.AppendSeparator();
        editMenu.AppendAction("Copy",  null);
        editMenu.AppendAction("Cut",   null);
        editMenu.AppendAction("Paste", null);

        root.Q<Button>("edit-button").clickable.clickedWithEventInfo +=
            evt => dropdown.Open(editMenu, evt);

        BuildViewMenu();
        root.Q<Button>("view-button").clickable.clickedWithEventInfo +=
            evt => dropdown.Open(viewMenu, evt);

        toolsMenu = new DropdownMenu();
        root.Q<Button>("tools-button").clickable.clickedWithEventInfo +=
            evt => dropdown.Open(toolsMenu, evt);
    }

    private void BuildViewMenu()
    {
        for (int i = 0; i < toggleableUIs.Count; i++)
            viewMenu.AppendAction(toggleableUIs[i].Name, toggleableUIs[i].ToggleUI);
    }

    // ─────────────────────────────────────────────────────────────
    // ADDED: Map saving/loading
    private void OnSaveClicked(DropdownMenuAction action)
    {
        // Prototype map system (DecorObjects + terrain + grid shader)
        var msl = MapSaveLoad.Instance;
        if (msl != null)
        {
            msl.SaveWithDialog();
            return;          // MapSaveLoad opens its own dialog
        }

        // Fallback: old hex-tile serializer
        if (placementSystem == null)
        {
            Debug.LogError("EditionToolbarUIManager: PlacementSystem is not assigned.");
            return;
        }

        var markers = Object.FindObjectsByType<PlacedTile>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
        if (string.IsNullOrEmpty(path)) { Debug.Log("Save cancelled."); return; }

        File.WriteAllText(path, _serializer.Serialize(data, true));
        Debug.Log($"Saved {data.tiles.Count} tiles to {path}");
    }

    // ── File → Import Map ─────────────────────────────────────────────────────
    private void OnLoadClicked(DropdownMenuAction action)
    {
        // Prototype map system
        var msl = MapSaveLoad.Instance;
        if (msl != null)
        {
            msl.LoadWithDialog();
            return;
        }

        // Fallback
        if (placementSystem == null)
        {
            Debug.LogError("EditionToolbarUIManager: PlacementSystem is not assigned.");
            return;
        }
        if (!placementSystem.HasGrid)
        {
            Debug.LogError("EditionToolbarUIManager: Grid not set on PlacementSystem.");
            return;
        }
        if (placementSystem.PrefabCount <= 0)
        {
            Debug.LogError("EditionToolbarUIManager: No prefabs set on PlacementSystem.");
            return;
        }

        var path = _dialog.OpenFile("Open Map", "json");
        if (string.IsNullOrEmpty(path)) { Debug.Log("Load cancelled."); return; }
        if (!File.Exists(path))         { Debug.LogError($"File not found: {path}"); return; }

        var data = _serializer.Deserialize(File.ReadAllText(path));
        if (data?.tiles == null)        { Debug.LogWarning("Load failed: invalid JSON."); return; }

        placementSystem.ClearAll();
        placementSystem.RebuildFrom(data);
        Debug.Log($"Loaded {data.tiles.Count} tiles from {path}");
    }

    // ── File → Import Asset ───────────────────────────────────────────────────
    private void OnImportAssetClicked(DropdownMenuAction action)
    {
        var aim = VTT.AssetImportManager.Instance;
        if (aim != null)
        {
            aim.ImportFromFileDialog();
            return;
        }

        // Fallback
        Debug.LogWarning("EditionToolbarUIManager: VTT.AssetImportManager not found. " +
            "Add the component to a scene GameObject.");
    }

    // ── Edit → Undo / Redo ───────────────────────────────────────────────────
    private void OnUndoClicked(DropdownMenuAction action)
    {
        var hist = CommandHistory.Instance;
        if (hist == null) { Debug.LogWarning("CommandHistory not found in scene."); return; }
        if (!hist.CanUndo) { Debug.Log("Nothing to undo."); return; }
        hist.Undo();
    }

    private void OnRedoClicked(DropdownMenuAction action)
    {
        var hist = CommandHistory.Instance;
        if (hist == null) { Debug.LogWarning("CommandHistory not found in scene."); return; }
        if (!hist.CanRedo) { Debug.Log("Nothing to redo."); return; }
        hist.Redo();
    }

    void IUIManager.ToggleUI(DropdownMenuAction action) { }
}