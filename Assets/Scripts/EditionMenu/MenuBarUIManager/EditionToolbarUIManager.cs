using UnityEngine;
using UnityEngine.UIElements;
using Fab.UITKDropdown;
using System.Collections.Generic;
using JetBrains.Annotations;
using HexGrid.Systems;
using HexGrid.IO;
using HexGrid.Models;
using HexGrid.Persistence;
using System.IO;

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

    string IUIManager.Name => UIName;

    public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme)
    {
        this.root = root;
        this.managedUI = managedUI;
        this.theme = theme;
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
        _serializer = new JsonMapSerializer();

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
        // create a dropdown and set the view element as its root
        dropdown = new Dropdown(root);

        // Create the File menu
        // To add a button functionality,
        // replace the "null" in each AppendAction with the function you want to run
        fileMenu = new DropdownMenu();
        fileMenu.AppendAction("Save", OnSaveClicked);
        fileMenu.AppendAction("Import Map", OnLoadClicked);
        fileMenu.AppendAction("Import Asset", null);
        fileMenu.AppendAction("Open/Rules", null);
        fileMenu.AppendAction("Open/Sheets", null);
        fileMenu.AppendAction("Link to object/Notes", null);
        fileMenu.AppendAction("Link to object/Sheets", null);

        root.Q<Button>("file-button").clickable.clickedWithEventInfo += evt => dropdown.Open(fileMenu, evt);

        // Create the Edit menu
        editMenu = new DropdownMenu();
        editMenu.AppendAction("Undo action", null);
        editMenu.AppendAction("Redo action", null);
        editMenu.AppendSeparator();
        editMenu.AppendAction("Copy", null);
        editMenu.AppendAction("Cut", null);
        editMenu.AppendAction("Paste", null);

        root.Q<Button>("edit-button").clickable.clickedWithEventInfo += evt => dropdown.Open(editMenu, evt);

        BuildViewMenu();

        root.Q<Button>("view-button").clickable.clickedWithEventInfo += evt => dropdown.Open(viewMenu, evt);

        // Create the Tools menu
        toolsMenu = new DropdownMenu();

        root.Q<Button>("tools-button").clickable.clickedWithEventInfo += evt => dropdown.Open(toolsMenu, evt);
    }

    private void BuildViewMenu()
    {
        for (int i = 0; i < toggleableUIs.Count; i++)
        {
            viewMenu.AppendAction(toggleableUIs[i].Name, toggleableUIs[i].ToggleUI);
        }
    }

    private void OnSaveClicked(DropdownMenuAction action)
    {
        if (placementSystem == null)
        {
            Debug.LogError("EditionToolbarUIManager: PlacementSystem is not assigned.");
            return;
        }

        var markers = Object.FindObjectsByType<PlacedTile>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var data = new MapDataDTO();
        
        foreach (var m in markers)
        {
            data.tiles.Add(new PlacedTileDTO
            {
                prefabIndex = Mathf.Clamp(m.prefabIndex, 0, placementSystem.PrefabCount - 1),
                x = m.cell.x,
                y = m.cell.y,
                z = m.cell.z,
                yRotation = m.yRotation
            });
        }

        var path = _dialog.SaveFile("Save Map As", "map.json", "json");
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Save cancelled by user.");
            return;
        }

        var json = _serializer.Serialize(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"Saved {data.tiles.Count} tiles to {path}");
    }

    private void OnLoadClicked(DropdownMenuAction action)
    {
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
        if (string.IsNullOrEmpty(path))
        {
            Debug.Log("Load cancelled by user.");
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            return;
        }

        var json = File.ReadAllText(path);
        var data = _serializer.Deserialize(json);
        
        if (data == null || data.tiles == null)
        {
            Debug.LogWarning("Load failed: invalid JSON.");
            return;
        }

        placementSystem.ClearAll();
        placementSystem.RebuildFrom(data);
        Debug.Log($"Loaded {data.tiles.Count} tiles from {path}");
    }

    void IUIManager.ToggleUI(DropdownMenuAction action)
    {
        return;
    }
}