using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class ConfigFoldout : MonoBehaviour
    {
        [SerializeField] private Button header;
        [SerializeField] private RectTransform arrow;
        [SerializeField] private GameObject content;
        [SerializeField] private bool expanded = true;

        private void Awake()
        {
            header.onClick.AddListener(Toggle);
            Apply();
        }

        private void Toggle()
        {
            expanded = !expanded;
            Apply();
        }

        private void Apply()
        {
            content.SetActive(expanded);
            arrow.localEulerAngles = new Vector3(0f, 0f, expanded ? -90f : 0f);
        }
    }
}
