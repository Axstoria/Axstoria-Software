using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CharacterSheet.Presenter.View
{
    public class ElementsPanelController : MonoBehaviour
    {
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private RectTransform content;

        private readonly List<(GameObject Item, string Label)> _items = new();

        private void Start()
        {
            if (content != null) {
                for (int i = 0; i < content.childCount; i++) {
                    var child = content.GetChild(i).gameObject;
                    if (!child.activeSelf) continue;
                    if (child.GetComponent<Button>() == null) continue;
                    var label = child.GetComponentInChildren<TMP_Text>(true);
                    _items.Add((child, label != null ? label.text.ToLowerInvariant() : string.Empty));
                }
            }

            if (searchField != null)
                searchField.onValueChanged.AddListener(Filter);
        }

        private void Filter(string query)
        {
            string q = query != null ? query.Trim().ToLowerInvariant() : string.Empty;
            foreach (var (item, label) in _items)
                item.SetActive(q.Length == 0 || label.Contains(q));
        }
    }
}
