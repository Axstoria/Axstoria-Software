using System;
using System.Collections.Generic;
using System.IO;
using App.Domain;
using Fab.UITKDropdown;
using Loxodon.Framework.Contexts;
using MapEditor.Domain;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private DropdownMenu helpMenu;

        private Label _labelViewingAs;
        private MapEditorViewModel _vm;

        private List<IUIManager> toggleableUIs = new List<IUIManager>();
        private List<(string Name, Action<DropdownMenuAction> Callback, Func<DropdownMenuAction, DropdownMenuAction.Status> Status)> extraViewEntries = new();

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

        public void AddViewMenuEntry(string name, Action<DropdownMenuAction> callback,
                                     Func<DropdownMenuAction, DropdownMenuAction.Status> status = null)
        {
            extraViewEntries.Add((name, callback, status));
        }

        private void Start()
        {
            dropdown = new Dropdown(root);

            fileMenu = new DropdownMenu();
            fileMenu.AppendAction("Switch to Sheet Editor", OnSwitchToSheetEditorClicked);
            fileMenu.AppendAction("Back to Menu",           OnBackToMenuClicked);
            fileMenu.AppendSeparator();
            fileMenu.AppendAction("Save",                  OnSaveClicked);
            fileMenu.AppendAction("Import Map",            OnImportMapClicked);
            fileMenu.AppendAction("Import Asset",          OnImportAssetClicked);
            fileMenu.AppendAction("Open/Rules",            null);
            fileMenu.AppendAction("Open/Sheets",           null);
            fileMenu.AppendAction("Link to object",  OnLinkToObjectClicked);

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

            root.Q<Button>("tools-button").clickable.clickedWithEventInfo +=
                evt => dropdown.Open(BuildToolsMenu(), evt);

            helpMenu = new DropdownMenu();
            helpMenu.AppendAction("About", null);
            root.Q<Button>("help-button").clickable.clickedWithEventInfo +=
                evt => dropdown.Open(helpMenu, evt);

            _labelViewingAs = root.Q<Label>("label-viewing-as");
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm != null)
            {
                _vm.ViewingAsLabel.ValueChanged += (_, __) => RefreshViewingAsLabel();
                RefreshViewingAsLabel();
            }
        }

        private void RefreshViewingAsLabel()
        {
            if (_labelViewingAs == null) return;
            _labelViewingAs.text = _vm.ViewingAsLabel.Value;
            _labelViewingAs.style.display =
                string.IsNullOrEmpty(_vm.ViewingAsLabel.Value) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private DropdownMenu BuildToolsMenu()
        {
            var menu = new DropdownMenu();
            if (_vm == null) return menu;

            string currentId = _vm.Session.CurrentPlayer?.IsGameMaster == true
                ? null
                : _vm.Session.CurrentPlayer?.Id;

            menu.AppendAction("Preview as/Game Master", _ => _vm.Session.SetCurrentPlayer(null),
                _ => string.IsNullOrEmpty(currentId) ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            foreach (PlayerViewModel player in _vm.Map.Players)
            {
                string id = player.Model.Id;
                menu.AppendAction($"Preview as/{player.Name.Value}", _ => _vm.Session.SetCurrentPlayer(id),
                    _ => currentId == id ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }

            return menu;
        }

        private void BuildViewMenu()
        {
            for (int i = 0; i < toggleableUIs.Count; i++)
                viewMenu.AppendAction(toggleableUIs[i].Name, toggleableUIs[i].ToggleUI);

            foreach (var entry in extraViewEntries)
            {
                if (entry.Status != null)
                    viewMenu.AppendAction(entry.Name, entry.Callback, entry.Status);
                else
                    viewMenu.AppendAction(entry.Name, entry.Callback);
            }
        }

        public Action OnLinkToObjectRequested;
        private void OnLinkToObjectClicked(DropdownMenuAction action)
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm == null)
            {
                Debug.LogWarning("[EditionToolbarUIManager] MapEditorViewModel not registered, cannot link.");
                return;
            }
            OnLinkToObjectRequested?.Invoke();
        }

        private void OnSwitchToSheetEditorClicked(DropdownMenuAction action)
        {
            NavigateTo(SceneNames.SheetEdition);
        }

        private void OnBackToMenuClicked(DropdownMenuAction action)
        {
            NavigateTo(SceneNames.EditionMenu);
        }

        private void NavigateTo(string sceneName)
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm != null) vm.SaveMap.Execute(vm.Map.Model);

            var navigation = Context.GetApplicationContext().GetContainer().Resolve<INavigationService>();
            if (navigation != null) navigation.LoadScene(sceneName);
            else SceneManager.LoadScene(sceneName);
        }

        private void OnImportAssetClicked(DropdownMenuAction action)
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm == null)
            {
                Debug.LogWarning("[EditionToolbarUIManager] MapEditorViewModel not registered, cannot import.");
                return;
            }
            vm.ImportAsset.Execute();
        }

        private void OnSaveClicked(DropdownMenuAction action)
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm == null)
            {
                Debug.LogWarning("[EditionToolbarUIManager] MapEditorViewModel not registered, cannot save.");
                return;
            }
            vm.SaveMap.Execute(vm.Map.Model);
        }

        private void OnImportMapClicked(DropdownMenuAction action)
        {
            var vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (vm == null)
            {
                Debug.LogWarning("[EditionToolbarUIManager] MapEditorViewModel not registered, cannot load.");
                return;
            }

            Map loaded = vm.LoadMap.Execute();
            if (loaded == null) return;

            Map active = vm.Map.Model;
            active.Id   = loaded.Id;
            active.Name = loaded.Name;

            var existing = new List<SceneObject>(active.Objects);
            foreach (SceneObject obj in existing)
                active.RemoveObject(obj);

            foreach (SceneObject obj in loaded.Objects)
                active.AddObject(obj);

            active.Metadata = loaded.Metadata;
            active.NotifyMetadataChanged();

            if (vm.Grid != null)
                vm.Grid.RebuildOccupancy(active.Objects);
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
