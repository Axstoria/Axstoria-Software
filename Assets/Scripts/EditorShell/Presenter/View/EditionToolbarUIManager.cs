using System.Collections.Generic;
using System.IO;
using Fab.UITKDropdown;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class EditionToolbarUIManager : MonoBehaviour, IUIManager
    {
        private ThemeStyleSheet theme;
        private VisualElement   root;
        private VisualElement   managedUI;
        private const string    UIName = "Toolbar";

        private Dropdown dropdown;

        private DropdownMenu fileMenu;
        private DropdownMenu editMenu;
        private DropdownMenu viewMenu = new();
        private DropdownMenu toolsMenu;

        private List<IUIManager> toggleableUIs = new List<IUIManager>();

        string IUIManager.Name => UIName;

        public void Init(VisualElement root, VisualElement managedUI, ThemeStyleSheet theme)
        {
            this.root      = root;
            this.managedUI = managedUI;
            this.theme     = theme;
            transform.position = Vector3.one;
        }

        public void AddToggleableUI(IUIManager element)
        {
            toggleableUIs.Add(element);
        }

        private void Start()
        {
            dropdown = new Dropdown(root);

            fileMenu = new DropdownMenu();
            fileMenu.AppendAction("Save",                  null);
            fileMenu.AppendAction("Import Map",            null);
            fileMenu.AppendAction("Import Asset",          OnImportAssetClicked);
            fileMenu.AppendAction("Open/Rules",            null);
            fileMenu.AppendAction("Open/Sheets",           null);
            fileMenu.AppendAction("Link to object/Notes",  null);
            fileMenu.AppendAction("Link to object/Sheets", null);

            root.Q<Button>("file-button").clickable.clickedWithEventInfo +=
                evt => dropdown.Open(fileMenu, evt);

            editMenu = new DropdownMenu();
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

        private void OnImportAssetClicked(DropdownMenuAction action)
        {
            // var aim = VTT.AssetImportManager.Instance;
            // if (aim != null)
            // {
            //     aim.ImportFromFileDialog();
            //     return;
            // }

            // Debug.LogWarning("EditionToolbarUIManager: VTT.AssetImportManager not found.");
        }

        private void OnUndoClicked(DropdownMenuAction action)
        {
            // var hist = CommandHistory.Instance;
            // if (hist == null) { Debug.LogWarning("CommandHistory not found in scene."); return; }
            // if (!hist.CanUndo) { Debug.Log("Nothing to undo."); return; }
            // hist.Undo();
        }

        private void OnRedoClicked(DropdownMenuAction action)
        {
            // var hist = CommandHistory.Instance;
            // if (hist == null) { Debug.LogWarning("CommandHistory not found in scene."); return; }
            // if (!hist.CanRedo) { Debug.Log("Nothing to redo."); return; }
            // hist.Redo();
        }

        void IUIManager.ToggleUI(DropdownMenuAction action) { }
    }
}
