using System.Collections.Generic;
using System.Collections.Specialized;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using SceneEditor.Presenter.ViewModels;
using UnityEngine;

namespace SceneEditor.Presenter.View
{
    public class SceneObjectSpawnerView : MonoBehaviour
    {
        [SerializeField] private Transform _container;

        private MapEditorViewModel _vm;
        private readonly Dictionary<string, GameObject>    _spawned         = new();
        private readonly Dictionary<string, ObjectViewModel> _pendingImported = new();

        private void Start()
        {
            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();
            if (_vm == null)
            {
                Debug.LogError("[SceneObjectSpawnerView] MapEditorViewModel not found. Ensure MapEditorBootstrapper runs first.");
                enabled = false;
                return;
            }

            ScenePrefabRegistry.OnPrefabRegistered += OnPrefabRegisteredById;
            _vm.Map.Objects.CollectionChanged      += OnObjectsChanged;

            foreach (ObjectViewModel objVm in _vm.Map.Objects)
                Spawn(objVm);
        }

        private void OnDestroy()
        {
            ScenePrefabRegistry.OnPrefabRegistered -= OnPrefabRegisteredById;

            if (_vm?.Map?.Objects != null)
                _vm.Map.Objects.CollectionChanged -= OnObjectsChanged;
        }

        private void OnObjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (ObjectViewModel objVm in e.NewItems)
                    Spawn(objVm);

            if (e.OldItems != null)
                foreach (ObjectViewModel objVm in e.OldItems)
                    Despawn(objVm.Model.Id);
        }

        private void Spawn(ObjectViewModel objVm)
        {
            SceneObject domain = objVm.Model;
            string id = domain.Id;
            if (string.IsNullOrEmpty(id) || _spawned.ContainsKey(id)) return;

            if (!ScenePrefabRegistry.TryGet(id, out GameObject prefab) || prefab == null)
            {
                if (!ScenePrefabRegistry.TryGetByCatalog(domain.Category, domain.DisplayName, out prefab) || prefab == null)
                {
                    // Imported asset — trigger async re-import; spawn when registry notifies us.
                    if (domain.IsImported && !string.IsNullOrEmpty(domain.ImportPath))
                    {
                        _pendingImported[id] = objVm;
                        _vm.ImportAsset.ExecuteFromPath(
                            domain.ImportPath, domain.Id, domain.Category, domain.DisplayName);
                        return;
                    }

                    Debug.LogWarning($"[SceneObjectSpawnerView] No prefab found for SceneObject '{id}' (category='{domain.Category}', name='{domain.DisplayName}'). Skipping spawn.");
                    return;
                }
            }

            SpawnWithPrefab(prefab, domain);
        }

        private void OnPrefabRegisteredById(string id)
        {
            if (!_pendingImported.TryGetValue(id, out ObjectViewModel objVm)) return;
            _pendingImported.Remove(id);
            Spawn(objVm);
        }

        private void SpawnWithPrefab(GameObject prefab, SceneObject domain)
        {
            Transform parent = _container != null ? _container : transform;
            GameObject instance = Instantiate(prefab, parent);
            instance.name = string.IsNullOrEmpty(domain.DisplayName) ? prefab.name : domain.DisplayName;

            SceneObjectView view = instance.GetComponent<SceneObjectView>() ?? instance.AddComponent<SceneObjectView>();
            view.Init(domain);

            _spawned[domain.Id] = instance;
        }

        public bool TryGetGameObject(string id, out GameObject go)
            => _spawned.TryGetValue(id, out go) && go != null;

        public bool TryGetId(GameObject go, out string id)
        {
            // Walk up the hierarchy so clicking a child mesh still finds the spawned root
            Transform t = go != null ? go.transform : null;
            while (t != null)
            {
                foreach (var kvp in _spawned)
                    if (kvp.Value == t.gameObject) { id = kvp.Key; return true; }
                t = t.parent;
            }
            id = null;
            return false;
        }

        private void Despawn(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            _pendingImported.Remove(id);
            if (_spawned.TryGetValue(id, out GameObject go))
            {
                if (go != null) Destroy(go);
                _spawned.Remove(id);
            }
        }
    }
}
