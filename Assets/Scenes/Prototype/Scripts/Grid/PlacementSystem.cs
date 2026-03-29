using UnityEngine;

namespace VTT.Grid
{
    [AddComponentMenu("VTT/Placement System")]
    public class PlacementSystem : MonoBehaviour
    {
        public static PlacementSystem Instance { get; private set; }

        [SerializeField] private Material validPreviewMaterial;
        [SerializeField] private Material invalidPreviewMaterial;

        private GameObject      _previewObject;
        private PlaceableObject _pendingPlaceable;

        public event System.Action<PlaceableObject, Vector2Int> OnObjectPlaced;
        public event System.Action<PlaceableObject, Vector2Int> OnObjectRemoved;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool CanPlace(PlaceableObject placeable, Vector2Int origin)
        {
            var gm = GridManager.Instance;
            foreach (var offset in placeable.FootprintOffsets)
            {
                var coord = origin + offset;
                if (!gm.GetCell(coord.x, coord.y).IsEmpty) return false;
            }
            return true;
        }

        public bool Place(PlaceableObject placeable, Vector2Int origin)
        {
            if (!CanPlace(placeable, origin)) return false;

            var gm       = GridManager.Instance;
            var worldPos = gm.GridToWorld(origin);
            worldPos.y   = placeable.transform.position.y;

            placeable.transform.position = worldPos;
            placeable.OccupiedOrigin     = origin;

            foreach (var offset in placeable.FootprintOffsets)
            {
                var coord = origin + offset;
                gm.PlaceObject(coord.x, coord.y, placeable.gameObject);
            }

            placeable.OnPlaced();
            OnObjectPlaced?.Invoke(placeable, origin);
            return true;
        }

        public void Remove(PlaceableObject placeable)
        {
            if (!placeable.IsPlaced) return;

            var gm     = GridManager.Instance;
            var origin = placeable.OccupiedOrigin;

            foreach (var offset in placeable.FootprintOffsets)
            {
                var coord = origin + offset;
                gm.RemoveObject(coord.x, coord.y);
            }

            placeable.OnRemoved();
            OnObjectRemoved?.Invoke(placeable, origin);
        }

        public void BeginPreview(PlaceableObject placeable, GameObject previewPrefab)
        {
            _pendingPlaceable = placeable;
            _previewObject    = Instantiate(previewPrefab);
        }

        public void UpdatePreview(Vector2Int hoveredCell)
        {
            if (_previewObject == null || _pendingPlaceable == null) return;
            _previewObject.transform.position = GridManager.Instance.GridToWorld(hoveredCell);
            SetPreviewMaterial(CanPlace(_pendingPlaceable, hoveredCell)
                ? validPreviewMaterial : invalidPreviewMaterial);
        }

        public bool ConfirmPreview(Vector2Int origin)
        {
            bool ok = Place(_pendingPlaceable, origin);
            CancelPreview();
            return ok;
        }

        public void CancelPreview()
        {
            if (_previewObject != null) Destroy(_previewObject);
            _previewObject    = null;
            _pendingPlaceable = null;
        }

        private void SetPreviewMaterial(Material mat)
        {
            if (mat == null) return;
            foreach (var r in _previewObject.GetComponentsInChildren<Renderer>())
                r.material = mat;
        }
    }
}
