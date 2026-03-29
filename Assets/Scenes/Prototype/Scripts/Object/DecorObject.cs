using System.Collections.Generic;
using UnityEngine;

namespace VTT
{
    /// <summary>
    /// Attach to any object placed by the VTT placement system.
    /// Maintains a global static registry so the Outliner and Gizmo can track all scene objects.
    /// </summary>
    [AddComponentMenu("VTT/Decor Object")]
    public class DecorObject : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────
        [Tooltip("Display name shown in the Outliner.")]
        public string displayName = "";

        [Tooltip("Category this object belongs to (e.g. 'Environment', 'Characters').")]
        public string category = "Uncategorized";

        [Tooltip("Original prefab name — set automatically on placement.")]
        public string prefabName = "";

        [Tooltip("True if this object was imported at runtime (glTF/GLB).")]
        public bool   isImported = false;

        [Tooltip("Absolute file path of the source model — used to re-import on load.")]
        public string importPath = "";

        [Tooltip("Grid cell this object occupies — set automatically on placement.")]
        public Vector2Int gridCell;

        // ── Global registry ───────────────────────────────────────────────────
        private static readonly List<DecorObject> _all = new();
        public  static IReadOnlyList<DecorObject>  All => _all;

        /// <summary>Fired whenever the registry changes (object added or removed).</summary>
        public static event System.Action OnRegistryChanged;

        // ── Lifecycle ─────────────────────────────────────────────────────────
        private void OnEnable()
        {
            if (!_all.Contains(this)) _all.Add(this);
            if (string.IsNullOrEmpty(displayName)) displayName = gameObject.name;
            OnRegistryChanged?.Invoke();
        }

        private void OnDisable()
        {
            _all.Remove(this);
            OnRegistryChanged?.Invoke();
        }

        private void OnDestroy()
        {
            _all.Remove(this);
            OnRegistryChanged?.Invoke();
        }

        // ── Public API ────────────────────────────────────────────────────────
        /// <summary>Rename this object (updates GameObject name too).</summary>
        public void Rename(string newName)
        {
            displayName      = newName;
            gameObject.name  = newName;
        }

        /// <summary>Remove from scene and free the grid cell.</summary>
        public void Delete()
        {
            var ps = Grid.PlacementSystem.Instance;
            var po = GetComponent<Grid.PlaceableObject>();
            if (ps != null && po != null) ps.Remove(po);
            Destroy(gameObject);
        }
    }
}