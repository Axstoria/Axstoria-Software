using UnityEngine;
using UnityEngine.UIElements;

namespace EditorShell.Presenter.View
{
    public static class UIPointerUtility
    {
        public static bool IsOverUI(IPanel panel, VisualElement backgroundRoot, Vector2 screenPosition)
        {
            VisualElement picked = Pick(panel, screenPosition);
            return picked != null && picked != backgroundRoot;
        }
        public static bool IsOverElement(IPanel panel, VisualElement target, Vector2 screenPosition)
        {
            if (target == null) return false;

            VisualElement picked = Pick(panel, screenPosition);
            while (picked != null)
            {
                if (picked == target) return true;
                picked = picked.parent;
            }
            return false;
        }

        private static VisualElement Pick(IPanel panel, Vector2 screenPosition)
        {
            if (panel == null) return null;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            return panel.Pick(panelPos);
        }
    }
}
