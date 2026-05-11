using System.Collections.Generic;
using UnityEngine;

namespace SceneEditor.Presenter
{
    public static class ScenePrefabRegistry
    {
        private static readonly Dictionary<string, GameObject> _byId = new();

        public static void Register(string id, GameObject prefab)
        {
            if (string.IsNullOrEmpty(id) || prefab == null) return;
            _byId[id] = prefab;
        }

        public static bool TryGet(string id, out GameObject prefab) =>
            _byId.TryGetValue(id, out prefab);

        public static void Unregister(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            _byId.Remove(id);
        }

        public static void Clear() => _byId.Clear();
    }
}
