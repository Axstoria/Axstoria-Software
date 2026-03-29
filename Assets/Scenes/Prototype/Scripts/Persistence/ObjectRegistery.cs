using System.Collections.Generic;
using UnityEngine;

namespace VTT.Persistence
{
    [CreateAssetMenu(menuName = "VTT/Prefab Registry", fileName = "PrefabRegistry")]
    public class PrefabRegistry : ScriptableObject
    {
        [System.Serializable]
        public class Entry
        {
            public string     key;
            public GameObject prefab;
        }

        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, GameObject> _lookup;

        public GameObject Find(string key)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(key, out var go);
            return go;
        }

        public void Register(string key, GameObject prefab)
        {
            if (_lookup == null) BuildLookup();
            if (!string.IsNullOrEmpty(key) && prefab != null)
                _lookup[key] = prefab;
        }

        /// <summary>
        /// Bulk-register from a prefab
        /// </summary>
        public void RegisterFromCategories(IEnumerable<UI.PrefabCategory> cats)
        {
            if (_lookup == null) BuildLookup();
            foreach (var cat in cats)
                if (cat?.prefabs != null)
                    foreach (var pf in cat.prefabs)
                        if (pf != null) _lookup[pf.name] = pf;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, GameObject>(entries.Count);
            foreach (var e in entries)
                if (e?.prefab != null && !string.IsNullOrEmpty(e.key))
                    _lookup[e.key] = e.prefab;
        }
    }
}
