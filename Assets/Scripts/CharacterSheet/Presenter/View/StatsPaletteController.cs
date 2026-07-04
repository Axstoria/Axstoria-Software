using System.Collections.Generic;
using CharacterSheet.App.UseCase;
using Loxodon.Framework.Contexts;
using TMPro;
using UnityEngine;

namespace CharacterSheet.Presenter.View
{
    public class StatsPaletteController : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private TMP_InputField searchField;

        // TODO: get stats list directly from repository
        private static readonly string[] KnownStatIds = { "hp", "force", "agility", "intelligence", "social", "intel", "wisdom", "constitution" };

        private readonly List<(GameObject Row, string Label)> _rows = new();

        private void Start()
        {
            var getStat = Context.GetApplicationContext().GetContainer().Resolve<GetStatUseCase>();
            if (getStat == null) return;

            foreach (var statId in KnownStatIds) {
                var stat = getStat.Execute(statId);
                if (stat == null) continue;
                var row = Instantiate(rowTemplate, container);
                var dragSource = row.GetComponent<StatDragSource>();
                if (dragSource != null) {
                    dragSource.StatId = statId;
                    dragSource.DisplayName = stat.Name;
                }
                var name = row.transform.Find("StatName");
                if (name != null) name.GetComponent<TMP_Text>().text = stat.Name;
                var badge = row.transform.Find("TypeBadge");
                if (badge != null) badge.GetComponent<TMP_Text>().text = stat.Type.ToString();
                row.SetActive(true);
                _rows.Add((row, stat.Name.ToLowerInvariant()));
            }

            if (searchField != null)
                searchField.onValueChanged.AddListener(Filter);
        }

        private void Filter(string query)
        {
            string q = query != null ? query.Trim().ToLowerInvariant() : string.Empty;
            foreach (var (row, label) in _rows)
                row.SetActive(q.Length == 0 || label.Contains(q));
        }
    }
}
