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
        }
    }
}
