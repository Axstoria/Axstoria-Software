using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LayoutUIManager : MonoBehaviour
{
    public UIDocument uiDoc;
    private EditionToolbarUIManager toolbarManager;
    private ViewportUIManager viewportManager;
    private ThemeStyleSheet theme;

    void Start()
    {
        VisualElement root = uiDoc.rootVisualElement;
        theme = uiDoc.panelSettings.themeStyleSheet;

        // Load and instantiate the new UI Document, then add it to this UI Document's root.
        // To see how to determine the path for loading the asset, read https://docs.unity3d.com/ScriptReference/Resources.Load.html
        VisualTreeAsset toolbarAsset = Resources.Load<VisualTreeAsset>("EditionToolbar/EditionToolbarUI");
        VisualElement toolbar = toolbarAsset.Instantiate();
        root.Add(toolbar);

        // Add the script managing the new UI Document
        // and initialize it with this document root and theme
        toolbarManager = this.AddComponent<EditionToolbarUIManager>();
        toolbarManager.Init(root, toolbar, theme);

        VisualTreeAsset viewportAsset = Resources.Load<VisualTreeAsset>("Viewport/Viewport");
        VisualElement viewport = viewportAsset.Instantiate();
        viewport.style.top = toolbar.style.bottom;
        root.Add(viewport);

        viewportManager = this.AddComponent<ViewportUIManager>();
        viewportManager.Init(root, viewport, theme);
        // Add the Viewport UI Manager to the list of UI elements that can be added to or removed from the layout.
        toolbarManager.AddToggleableUI(viewportManager);
    }
}
