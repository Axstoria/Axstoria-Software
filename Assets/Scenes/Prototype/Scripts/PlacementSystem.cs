using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// Handles placing and removing VTT objects (tokens, terrain pieces, props).
    /// Supports multi-cell footprints (e.g. a 2x2 creature base).
    /// </summary>
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance { get; private set; }

        [Header("Preview")]
        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        private GameObject _previewObject;
        private PlaceableObject _pendingPlaceable;

        // ── Events ──────────────────────────────────────────────────────────
        public event System.Action<PlaceableObject, Vector2Int> OnObjectPlaced;
        public event System.Action<PlaceableObject, Vector2Int> OnObjectRemoved;

        // ── Lifecycle ────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Placement validation ─────────────────────────────────────────────

        /// <summary>
        /// Check whether a placeable fits at the given grid origin.
        /// Returns true if all required cells are empty.
        /// </summary>
        public bool CanPlace(PlaceableObject placeable, Vector2Int origin)
        {
            GridManager gm = GridManager.Instance;
            foreach (Vector2Int offset in placeable.FootprintOffsets)
            {
                Vector2Int coord = origin + offset;
                GridCell cell = gm.GetCell(coord.x, coord.y);
                if (!cell.IsEmpty) return false;
            }
            return true;
        }

        /// <summary>Place a placeable at the given origin. Returns success.</summary>
        public bool Place(PlaceableObject placeable, Vector2Int origin)
        {
            if (!CanPlace(placeable, origin)) return false;

            GridManager gm = GridManager.Instance;
            Vector3 worldPos = gm.GridToWorld(origin);
            worldPos.y = placeable.transform.position.y; // keep original Y

            placeable.transform.position = worldPos;
            placeable.OccupiedOrigin = origin;

            foreach (Vector2Int offset in placeable.FootprintOffsets)
            {
                Vector2Int coord = origin + offset;
                gm.PlaceObject(coord.x, coord.y, placeable.gameObject);
            }

            placeable.OnPlaced();
            OnObjectPlaced?.Invoke(placeable, origin);
            return true;
        }

        /// <summary>Remove a placed object and free its cells.</summary>
        public void Remove(PlaceableObject placeable)
        {
            if (!placeable.IsPlaced) return;

            GridManager gm = GridManager.Instance;
            Vector2Int origin = placeable.OccupiedOrigin;

            foreach (Vector2Int offset in placeable.FootprintOffsets)
            {
                Vector2Int coord = origin + offset;
                gm.RemoveObject(coord.x, coord.y);
            }

            placeable.OnRemoved();
            OnObjectRemoved?.Invoke(placeable, origin);
        }

        // ── Preview / drag mode ──────────────────────────────────────────────

        public void BeginPreview(PlaceableObject placeable, GameObject previewPrefab)
        {
            _pendingPlaceable = placeable;
            _previewObject = Instantiate(previewPrefab);
        }

        public void UpdatePreview(Vector2Int hoveredCell)
        {
            if (_previewObject == null || _pendingPlaceable == null) return;

            GridManager gm = GridManager.Instance;
            _previewObject.transform.position = gm.GridToWorld(hoveredCell);

            bool valid = CanPlace(_pendingPlaceable, hoveredCell);
            SetPreviewMaterial(valid ? validPreviewMaterial : invalidPreviewMaterial);
        }

        public bool ConfirmPreview(Vector2Int origin)
        {
            bool success = Place(_pendingPlaceable, origin);
            CancelPreview();
            return success;
        }

        public void CancelPreview()
        {
            if (_previewObject != null) Destroy(_previewObject);
            _previewObject     = null;
            _pendingPlaceable  = null;
        }

        private void SetPreviewMaterial(Material mat)
        {
            if (mat == null) return;
            foreach (var r in _previewObject.GetComponentsInChildren<Renderer>())
                r.material = mat;
        }
    }

    // ── PlaceableObject component ─────────────────────────────────────────────

    /// <summary>
    /// Attach to any VTT token or prop. Defines its cell footprint.
    /// </summary>
    public class PlaceableObject : MonoBehaviour
    {
        [Tooltip("Cell offsets relative to origin. (0,0) = origin cell. " +
                 "A 2x2 token uses (0,0),(1,0),(0,1),(1,1).")]
        public List<Vector2Int> FootprintOffsets = new() { Vector2Int.zero };

        public bool      IsPlaced       { get; private set; }
        public Vector2Int OccupiedOrigin { get; set; }

        public virtual void OnPlaced()  { IsPlaced = true; }
        public virtual void OnRemoved() { IsPlaced = false; }
    }
}
