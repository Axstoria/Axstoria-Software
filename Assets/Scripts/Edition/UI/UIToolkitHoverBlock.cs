using UnityEngine;
using UnityEngine.UIElements;

namespace HexGrid.UI
{
    /// Detects hover over interactive UI controls. Here: Buttons only.
    public static class UIToolkitHoverBlock
    {
        public static bool IsPointerOverAnyButton()
        {
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (docs == null || docs.Length == 0) return false;

            Vector2 screenPos = Input.mousePosition;

            foreach (var doc in docs)
            {
                if (doc == null || doc.rootVisualElement == null) continue;
                var panel = doc.rootVisualElement.panel;
                if (panel == null) continue;

                // Convert to this panel's coordinates
                Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

                // Pick topmost element under the pointer in this panel
                var picked = panel.Pick(panelPos);
                if (picked == null) continue;

                // Walk up the hierarchy; if we find a Button, block.
                for (var ve = picked; ve != null && ve != doc.rootVisualElement; ve = ve.parent)
                {
                    if (ve is Button btn && btn.visible && btn.enabledInHierarchy)
                        return true;
                }
            }

            return false;
        }
    }
}
