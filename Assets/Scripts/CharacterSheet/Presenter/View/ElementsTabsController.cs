using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class ElementsTabsController : MonoBehaviour
    {
        private static readonly Color ActiveBg = new Color32(50, 50, 50, 255);
        private static readonly Color InactiveBg = new Color32(42, 42, 42, 255);
        private static readonly Color ActiveText = new Color32(235, 235, 235, 255);
        private static readonly Color InactiveText = new Color32(160, 160, 160, 255);

        [SerializeField] private Button widgetsTab;
        [SerializeField] private Button statsTab;
        [SerializeField] private Image widgetsTabBackground;
        [SerializeField] private Image statsTabBackground;
        [SerializeField] private TMP_Text widgetsTabLabel;
        [SerializeField] private TMP_Text statsTabLabel;
        [SerializeField] private GameObject[] widgetsObjects;
        [SerializeField] private GameObject[] statsObjects;

        private void Awake()
        {
            widgetsTab.onClick.AddListener(() => Select(true));
            statsTab.onClick.AddListener(() => Select(false));
            Select(true);
        }

        private void Select(bool widgets)
        {
            foreach (var go in widgetsObjects) go.SetActive(widgets);
            foreach (var go in statsObjects) go.SetActive(!widgets);
            widgetsTabBackground.color = widgets ? ActiveBg : InactiveBg;
            statsTabBackground.color = widgets ? InactiveBg : ActiveBg;
            widgetsTabLabel.color = widgets ? ActiveText : InactiveText;
            statsTabLabel.color = widgets ? InactiveText : ActiveText;
        }
    }
}
