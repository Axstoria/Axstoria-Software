using UnityEngine;
using UnityEngine.UIElements;
using Fab.UITKDropdown;
using System.Collections.Generic;
using JetBrains.Annotations;

public class EditionToolbarUIManager : MonoBehaviour, IUIManager
{
    private ThemeStyleSheet theme;
    private VisualElement root;
    private VisualElement managedUI;
    private const string UIName = "Toolbar";

    private Dropdown dropdown;

    private DropdownMenu fileMenu;
    private DropdownMenu editMenu;
    private DropdownMenu viewMenu = new();
    private DropdownMenu toolsMenu;

    private List<IUIManager> toggleableUIs = new List<IUIManager>();

    string IUIManager.Name => UIName;

    public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme)
    {
        this.root = root;
        this.managedUI = managedUI;
        this.theme = theme;
    }

    public void AddToggleableUI(IUIManager element)
    {
        toggleableUIs.Add(element);
    }

    private void Start()
    {
        // create a dropdown and set the view element as its root
        dropdown = new Dropdown(root);

        // Create the File menu
        // To add a button functionality,
        // replace the "null" in each AppendAction with the function you want to run
        fileMenu = new DropdownMenu();
        fileMenu.AppendAction("Save", null);
        fileMenu.AppendAction("Import Map", null);
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

    void IUIManager.ToggleUI(DropdownMenuAction action)
    {
        return;
    }
}