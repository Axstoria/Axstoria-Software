using System.Collections.Generic;
using UnityEngine;

namespace VTT.Persistence
{
    /// <summary>
    /// Maps prefab names → GameObject references so MapLoader can
    /// re-instantiate placed objects by name after loading a save file.
    ///
    /// Create one via: Assets → right-click → Create → VTT → Prefab Registry
    /// Assign all your prefabs, then drag the asset into MapSaveLoad in the Inspector.
    ///
    /// At runtime, VTTPanelUI categories are also registered automatically
    /// so imported models don't need manual registration.
    /// </summary>
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

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, GameObject>(entries.Count);
            foreach (var e in entries)
                if (e?.prefab != null && !string.IsNullOrEmpty(e.key))
                    _lookup[e.key] = e.prefab;
        }

        /// <summary>Find a prefab by name. Returns null if not registered.</summary>
        public GameObject Find(string key)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(key, out var go);
            return go;
        }

        /// <summary>Register a prefab at runtime (e.g. imported glTF models).</summary>
        public void Register(string key, GameObject prefab)
        {
            if (_lookup == null) BuildLookup();
            if (!string.IsNullOrEmpty(key) && prefab != null)
                _lookup[key] = prefab;
        }

        /// <summary>Register all prefabs from a VTTPanelUI category list.</summary>
        public void RegisterFromCategories(System.Collections.Generic.IEnumerable<UI.PrefabCategory> cats)
        {
            if (_lookup == null) BuildLookup();
            foreach (var cat in cats)
                if (cat?.prefabs != null)
                    foreach (var pf in cat.prefabs)
                        if (pf != null) _lookup[pf.name] = pf;
        }
    }
}