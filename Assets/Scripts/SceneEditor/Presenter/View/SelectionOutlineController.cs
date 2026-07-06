using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Loxodon.Framework.Contexts;
using MapEditor.Presenter.View;
using MapEditor.Presenter.ViewModels;
using SceneEditor.Domain;
using UnityEngine;

namespace SceneEditor.Presenter.View
{
    public class SelectionOutlineController : MonoBehaviour
    {
        private static readonly Color DefaultOutlineColor = new(1f, 0.5f, 0f, 1f);
        private static Material _outlineMaterial;

        private TransformGizmoView _gizmo;
        private SceneObjectSpawnerView _spawner;
        private MapEditorViewModel _vm;

        private SceneObject _currentDomainObj;
        private GameObject _outlinedObject;
        private bool? _lastSelectModeActive;
        private readonly List<Material> _materialsBuffer = new();

        private NotifyCollectionChangedEventHandler _onPlayersChanged;
        private readonly Dictionary<PlayerViewModel, EventHandler> _colorHandlers = new();

        private void Start()
        {
            _gizmo = FindFirstObjectByType<TransformGizmoView>();
            _spawner = FindFirstObjectByType<SceneObjectSpawnerView>();

            if (_gizmo == null || _spawner == null)
            {
                Debug.LogWarning("[SelectionOutlineController] TransformGizmoView/SceneObjectSpawnerView not found.");
                enabled = false;
                return;
            }

            _vm = Context.GetApplicationContext().GetContainer().Resolve<MapEditorViewModel>();

            if (_outlineMaterial == null)
                _outlineMaterial = new Material(Resources.Load<Shader>("Outline"));

            _gizmo.OnSelectionChanged += OnSelectionChanged;

            if (_vm != null)
            {
                SubscribeToPlayerColors();
                _onPlayersChanged = (_, __) => SubscribeToPlayerColors();
                _vm.Map.Players.CollectionChanged += _onPlayersChanged;
            }
        }

        private void OnDestroy()
        {
            if (_gizmo != null) _gizmo.OnSelectionChanged -= OnSelectionChanged;
            if (_vm != null && _onPlayersChanged != null) _vm.Map.Players.CollectionChanged -= _onPlayersChanged;
            UnsubscribePlayerColors();
            ClearOutline();
        }

        private void SubscribeToPlayerColors()
        {
            UnsubscribePlayerColors();
            foreach (PlayerViewModel player in _vm.Map.Players)
            {
                EventHandler handler = (_, __) => RefreshOutline();
                player.HexColor.ValueChanged += handler;
                _colorHandlers[player] = handler;
            }
        }

        private void UnsubscribePlayerColors()
        {
            foreach (var kvp in _colorHandlers)
                kvp.Key.HexColor.ValueChanged -= kvp.Value;
            _colorHandlers.Clear();
        }

        private void Update()
        {
            if (_gizmo == null) return;

            bool selectMode = _gizmo.IsSelectModeActive;
            if (_lastSelectModeActive == selectMode) return;

            _lastSelectModeActive = selectMode;
            RefreshOutline();
        }

        private void OnSelectionChanged(SceneObject domainObj)
        {
            _currentDomainObj = domainObj;
            RefreshOutline();
        }

        private void RefreshOutline()
        {
            ClearOutline();

            if (_currentDomainObj == null) return;
            if (_gizmo == null || !_gizmo.IsSelectModeActive) return;
            if (!_spawner.TryGetGameObject(_currentDomainObj.Id, out GameObject go)) return;

            ApplyOutline(go);
        }

        private void ApplyOutline(GameObject go)
        {
            _outlinedObject = go;
            _outlineMaterial.SetColor("_OutlineColor", ResolveOutlineColor());

            foreach (Renderer renderer in go.GetComponentsInChildren<Renderer>(true))
            {
                _materialsBuffer.Clear();
                _materialsBuffer.AddRange(renderer.sharedMaterials);
                if (_materialsBuffer.Contains(_outlineMaterial)) continue;

                _materialsBuffer.Add(_outlineMaterial);
                renderer.materials = _materialsBuffer.ToArray();
            }
        }

        private Color ResolveOutlineColor()
        {
            if (_vm != null && _currentDomainObj != null && _currentDomainObj.IsPawn)
            {
                var owner = _vm.Map.Players.FirstOrDefault(p => p.Model.PawnId == _currentDomainObj.Id);
                if (owner != null && ColorUtility.TryParseHtmlString(owner.HexColor.Value, out Color c))
                    return c;
            }

            return DefaultOutlineColor;
        }

        private void ClearOutline()
        {
            if (_outlinedObject == null) return;

            foreach (Renderer renderer in _outlinedObject.GetComponentsInChildren<Renderer>(true))
            {
                _materialsBuffer.Clear();
                _materialsBuffer.AddRange(renderer.sharedMaterials);
                if (!_materialsBuffer.Contains(_outlineMaterial)) continue;

                _materialsBuffer.Remove(_outlineMaterial);
                renderer.materials = _materialsBuffer.ToArray();
            }

            _outlinedObject = null;
        }
    }
}
