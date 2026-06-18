using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public class SidePanelToggleController : MonoBehaviour
    {
        private VisualElement _settingsPane;
        private VisualElement _outlinerPane;
        private VisualElement _handleLeft;
        private VisualElement _handleRight;
        private Button        _settingsTab;
        private Button        _outlinerTab;

        public void Init(VisualElement root)
        {
            _settingsPane = root.Q<VisualElement>("settings-pane");
            _outlinerPane = root.Q<VisualElement>("outliner-pane");
            _handleLeft   = root.Q<VisualElement>("split-handle-left");
            _handleRight  = root.Q<VisualElement>("split-handle-right");
            _settingsTab  = root.Q<Button>("settings-reopen-tab");
            _outlinerTab  = root.Q<Button>("outliner-reopen-tab");

            Button settingsClose = root.Q<Button>("settings-collapse-btn");
            Button outlinerClose = root.Q<Button>("outliner-collapse-btn");

            if (settingsClose != null)
                settingsClose.clicked += () => SetCollapsed(_settingsPane, _handleLeft, _settingsTab, true);
            if (outlinerClose != null)
                outlinerClose.clicked += () => SetCollapsed(_outlinerPane, _handleRight, _outlinerTab, true);
            if (_settingsTab != null)
                _settingsTab.clicked += () => SetCollapsed(_settingsPane, _handleLeft, _settingsTab, false);
            if (_outlinerTab != null)
                _outlinerTab.clicked += () => SetCollapsed(_outlinerPane, _handleRight, _outlinerTab, false);
        }

        private static void SetCollapsed(VisualElement pane, VisualElement handle, VisualElement tab, bool collapsed)
        {
            if (pane == null) return;

            DisplayStyle paneDisplay = collapsed ? DisplayStyle.None : DisplayStyle.Flex;
            pane.style.display = paneDisplay;
            if (handle != null) handle.style.display = paneDisplay;
            if (tab != null) tab.style.display = collapsed ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
