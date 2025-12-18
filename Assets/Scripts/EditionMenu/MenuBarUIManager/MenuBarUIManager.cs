using UnityEngine;
using UnityEngine.UIElements;
using Fab.UITKDropdown;

public class MenubarSample : MonoBehaviour
{
    public UIDocument uiDoc;

    private ThemeStyleSheet defaultTheme;

    private Dropdown dropdown;

    private DropdownMenu fileMenu;
    private DropdownMenu editMenu;
    private DropdownMenu viewMenu;
    private DropdownMenu toolsMenu;

    private void Start()
    {
        defaultTheme = uiDoc.panelSettings.themeStyleSheet;
        
        VisualElement root = uiDoc.rootVisualElement;

        // create a dropdown and set the view element as its root
        dropdown = new Dropdown(root);

        // Create the File menu
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

        // Create the View menu
        viewMenu = new DropdownMenu();

        root.Q<Button>("view-button").clickable.clickedWithEventInfo += evt => dropdown.Open(viewMenu, evt);

        // Create the Tools menu
        toolsMenu = new DropdownMenu();

        root.Q<Button>("tools-button").clickable.clickedWithEventInfo += evt => dropdown.Open(toolsMenu, evt);
    }
}