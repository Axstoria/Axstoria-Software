using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class SheetPanelsController : MonoBehaviour
    {
        private const float PanelMin     = 160f;
        private const float WorkspaceMin = 280f;
        private const float SheetGap     = 4f;
        private const float HandleWidth  = 8f;

        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform workspace;
        [SerializeField] private Button leftCloseButton;
        [SerializeField] private Button rightCloseButton;

        public bool IsLeftPresent  => leftPanel != null && leftPanel.gameObject.activeSelf;
        public bool IsRightPresent => rightPanel != null && rightPanel.gameObject.activeSelf;

        public float PanelMinWidth => PanelMin;

        private void Awake()
        {
            if (leftCloseButton != null)
                leftCloseButton.onClick.AddListener(() => SetPanelOpen(leftPanel, false));
            if (rightCloseButton != null)
                rightCloseButton.onClick.AddListener(() => SetPanelOpen(rightPanel, false));

            UpdateWorkspace();
        }

        public void ToggleLeft()  => SetPanelOpen(leftPanel, !IsLeftPresent);
        public void ToggleRight() => SetPanelOpen(rightPanel, !IsRightPresent);

        public void SetPanelWidth(RectTransform panel, float width)
        {
            if (panel == null) return;
            panel.sizeDelta = new Vector2(width, panel.sizeDelta.y);
            UpdateWorkspace();
        }

        public float GetMaxWidth(RectTransform panel)
        {
            var parent = panel != null ? panel.parent as RectTransform : null;
            if (parent == null) return PanelMin;

            RectTransform other = panel == leftPanel ? rightPanel : leftPanel;
            float otherWidth = other != null && other.gameObject.activeSelf ? DisplayedWidth(other) : 0f;
            float scale = Mathf.Max(panel.localScale.x, 0.0001f);
            float max = (parent.rect.width - otherWidth - GetWorkspaceMinWidth() - HandleWidth * 2f) / scale;
            return Mathf.Max(max, PanelMin);
        }

        private float GetWorkspaceMinWidth()
        {
            if (workspace == null) return WorkspaceMin;

            for (int i = 0; i < workspace.childCount; i++) {
                var sheet = workspace.GetChild(i) as RectTransform;
                if (sheet == null || !sheet.gameObject.activeSelf) continue;
                return sheet.rect.width * workspace.localScale.x + SheetGap * 2f;
            }

            return WorkspaceMin;
        }

        private void SetPanelOpen(RectTransform panel, bool open)
        {
            if (panel == null) return;
            panel.gameObject.SetActive(open);
            UpdateWorkspace();
        }

        private void UpdateWorkspace()
        {
            if (workspace == null) return;
            float left  = IsLeftPresent  ? DisplayedWidth(leftPanel)  : 0f;
            float right = IsRightPresent ? DisplayedWidth(rightPanel) : 0f;
            workspace.offsetMin = new Vector2(left, workspace.offsetMin.y);
            workspace.offsetMax = new Vector2(-right, workspace.offsetMax.y);
        }

        private static float DisplayedWidth(RectTransform panel)
            => panel.sizeDelta.x * panel.localScale.x;
    }
}
