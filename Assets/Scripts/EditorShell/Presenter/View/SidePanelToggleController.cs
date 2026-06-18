using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SidePanelToggleController : MonoBehaviour
    {
        private class Panel
        {
            public VisualElement Pane;
            public VisualElement Handle;
            public VisualElement Tab;
        }

        private Panel _settings;
        private Panel _outliner;

        public void Init(VisualElement root)
        {
            _settings = BuildPanel(root, "settings-pane", "split-handle-left", "settings-reopen-tab",
                                   "settings-collapse-btn", "settings-close-btn");
            _outliner = BuildPanel(root, "outliner-pane", "split-handle-right", "outliner-reopen-tab",
                                   "outliner-collapse-btn", "outliner-close-btn");
        }

        public void ToggleSettings() => Toggle(_settings);
        public void ToggleOutliner() => Toggle(_outliner);

        public bool IsSettingsOpen => IsOpen(_settings);
        public bool IsOutlinerOpen => IsOpen(_outliner);

        private static bool IsOpen(Panel panel)
            => panel?.Pane != null && panel.Pane.resolvedStyle.display == DisplayStyle.Flex;

        private Panel BuildPanel(VisualElement root, string paneName, string handleName, string tabName,
                                 string collapseBtnName, string closeBtnName)
        {
            var panel = new Panel
            {
                Pane   = root.Q<VisualElement>(paneName),
                Handle = root.Q<VisualElement>(handleName),
                Tab    = root.Q<Button>(tabName),
            };

            Button collapse = root.Q<Button>(collapseBtnName);
            Button close    = root.Q<Button>(closeBtnName);

            if (collapse != null)         collapse.clicked += () => Collapse(panel);
            if (close != null)            close.clicked += () => Close(panel);
            if (panel.Tab is Button tab)  tab.clicked += () => Open(panel);

            return panel;
        }

        private void Toggle(Panel panel)
        {
            if (panel?.Pane == null) return;
            if (panel.Pane.resolvedStyle.display == DisplayStyle.Flex) Close(panel);
            else Open(panel);
        }

        private static void Open(Panel panel)     => SetState(panel, DisplayStyle.Flex, DisplayStyle.None);
        private static void Collapse(Panel panel)  => SetState(panel, DisplayStyle.None, DisplayStyle.Flex);
        private static void Close(Panel panel)     => SetState(panel, DisplayStyle.None, DisplayStyle.None);

        private static void SetState(Panel panel, DisplayStyle paneDisplay, DisplayStyle tabDisplay)
        {
            if (panel?.Pane == null) return;
            panel.Pane.style.display = paneDisplay;
            if (panel.Handle != null) panel.Handle.style.display = paneDisplay;
            if (panel.Tab != null) panel.Tab.style.display = tabDisplay;
        }
    }
}
