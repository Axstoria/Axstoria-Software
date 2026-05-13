using MapEditor.Presenter.View;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class LayoutUIManager : MonoBehaviour
    {
        public UIDocument uiDoc;
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
            toolbarManager.AddToggleableUI(viewportManager);

            this.AddComponent<ViewSwitcherController>().Init(root);
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
        }
    }
}
