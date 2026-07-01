using MapEditor.Presenter.View;
using SceneEditor.Presenter.View;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class LayoutUIManager : MonoBehaviour
    {
        public UIDocument uiDoc;
        [SerializeField] private Texture2D _resizeCursor;
        [Header("References")]
        private EditionToolbarUIManager toolbarManager;
        private ViewportUIManager       viewportManager;
        private ThemeStyleSheet         theme;

        void Start()
        {
            VisualElement root = uiDoc.rootVisualElement;
            theme = uiDoc.panelSettings.themeStyleSheet;

            VisualTreeAsset toolbarAsset = Resources.Load<VisualTreeAsset>("EditionToolbar/EditionToolbarUI");
            VisualElement   toolbar      = toolbarAsset.Instantiate();
            root.Add(toolbar);

            toolbarManager = this.AddComponent<EditionToolbarUIManager>();
            toolbarManager.Init(root, toolbar, theme);

            VisualTreeAsset viewportAsset = Resources.Load<VisualTreeAsset>("Viewport/Viewport");
            VisualElement   viewport      = viewportAsset.Instantiate();
            viewport.style.top = toolbar.style.bottom;
            root.Add(viewport);

            viewportManager = this.AddComponent<ViewportUIManager>();
            viewportManager.Init(root, viewport, theme);

            this.AddComponent<ViewSwitcherController>().Init(root);
            this.AddComponent<SnapToolbarController>().Init(root);
            this.AddComponent<ToolsBarController>().Init(root);
            var moveFlyout   = this.AddComponent<MoveFlyoutController>();
            var layersFlyout = this.AddComponent<LayersFlyoutController>();
            moveFlyout.Init(root);
            layersFlyout.Init(root);
            moveFlyout.OnFlyoutOpened   = () => layersFlyout.HideImmediately();
            layersFlyout.OnFlyoutOpened = () => moveFlyout.HideImmediately();

            var gizmoView = FindFirstObjectByType<TransformGizmoView>();
            moveFlyout.OnToolSelected = type => { if (gizmoView != null) gizmoView.SetTransformType(type); };
            var bottomPanel = GetComponentInChildren<BottomPanelController>() ?? this.AddComponent<BottomPanelController>();
            bottomPanel.Init(root);
            GetComponentInChildren<PrefabBrowserView>()?.Init(root);
            this.AddComponent<TooltipController>().Init(root);
            this.AddComponent<SideBarController>().Init(root);

            VisualElement outlinerPane = root.Q<VisualElement>("outliner-pane");
            if (outlinerPane != null)
                this.AddComponent<OutlinerView>().Init(outlinerPane);

            this.AddComponent<SelectedObjectPanelController>().Init(root, _resizeCursor);

            this.AddComponent<SplitLayoutController>().Init(root, _resizeCursor);

            var sidePanels = this.AddComponent<SidePanelToggleController>();
            sidePanels.Init(root);
            toolbarManager.AddViewMenuEntry("Settings", _ => sidePanels.ToggleSettings(),
                _ => sidePanels.IsSettingsPresent ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            toolbarManager.AddViewMenuEntry("Outliner", _ => sidePanels.ToggleOutliner(),
                _ => sidePanels.IsOutlinerPresent ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);

            var metadataPopup = GetComponentInChildren<MetadataPopupController>() ?? this.AddComponent<MetadataPopupController>();
            metadataPopup.Init(root);
            toolbarManager.OnLinkToObjectRequested = () => metadataPopup.Open();
        }
    }
}
