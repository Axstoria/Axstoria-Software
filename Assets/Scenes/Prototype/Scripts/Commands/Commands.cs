using System.Collections.Generic;
using UnityEngine;
using VTT.Grid;

namespace VTT
{
    /// <summary>
    /// Command pattern for undo/redo system.
    /// </summary>
    /// TODO : Refacto to reduce code duplication between similar commands (e.g. PlaceCommand and DeleteCommand have a lot of overlap).
    ///        Split into separate files if this one gets too large.

    // Skeleton for new commands:
    //
    //   public class MyCommand : ICommand
    //   {
    //       public string Label => "My action";
    //       public MyCommand(...) { /* capture before-state */ }
    //       public void Execute() { /* do the thing */ }
    //       public void Undo()    { /* revert */ }
    //       public void Redo()    => Execute();
    //   }


    // ── Placement ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Placing a DecorObject on the grid.
    /// Undo hides it and frees its cell; Redo restores it.
    /// The object is never destroyed — it is parked inactive so undo can restore it.
    /// TODO: TEST memory and reference management for edge cases (e.g. place, delete, undo delete, undo place).
    /// </summary>
    public class PlaceCommand : ICommand
    {
        public string Label { get; }

        private readonly GameObject      _instance;
        private readonly PlaceableObject _po;
        private readonly Vector2Int      _cell;
        private readonly Vector3         _worldPos;
        private readonly Quaternion      _rotation;
        private readonly Vector3         _scale;
        private readonly Transform       _parent;

        public PlaceCommand(GameObject instance, DecorObject decor,
                            PlaceableObject po, Vector2Int cell)
        {
            _instance = instance;
            _po       = po;
            _cell     = cell;
            _worldPos = instance.transform.position;
            _rotation = instance.transform.rotation;
            _scale    = instance.transform.localScale;
            _parent   = instance.transform.parent;
            Label     = $"Place {decor.displayName}";
        }

        public void Execute() { }

        public void Undo()
        {
            PlacementSystem.Instance?.Remove(_po);
            _instance.SetActive(false);
        }

        public void Redo()
        {
            _instance.SetActive(true);
            _instance.transform.SetPositionAndRotation(_worldPos, _rotation);
            _instance.transform.localScale = _scale;
            if (_parent != null) _instance.transform.SetParent(_parent, true);
            PlacementSystem.Instance?.Place(_po, _cell);
        }
    }


    // ── Deletion ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Parks the object inactive instead of destroying it so Undo can restore it.
    /// Capture state before calling Record() — Execute() deactivates immediately.
    /// </summary>
    public class DeleteCommand : ICommand
    {
        public string Label { get; }

        private readonly GameObject      _instance;
        private readonly PlaceableObject _po;
        private readonly Vector2Int      _cell;
        private readonly Vector3         _worldPos;
        private readonly Quaternion      _rotation;
        private readonly Vector3         _scale;
        private readonly Transform       _parent;

        public DeleteCommand(DecorObject decor)
        {
            _instance = decor.gameObject;
            _po       = decor.GetComponent<PlaceableObject>();
            _cell     = _po != null ? _po.OccupiedOrigin : Vector2Int.zero;
            _worldPos = decor.transform.position;
            _rotation = decor.transform.rotation;
            _scale    = decor.transform.localScale;
            _parent   = decor.transform.parent;
            Label     = $"Delete {decor.displayName}";
        }

        public void Execute()
        {
            PlacementSystem.Instance?.Remove(_po);
            _instance.SetActive(false);
        }

        public void Undo()
        {
            _instance.SetActive(true);
            _instance.transform.SetPositionAndRotation(_worldPos, _rotation);
            _instance.transform.localScale = _scale;
            if (_parent != null) _instance.transform.SetParent(_parent, true);
            PlacementSystem.Instance?.Place(_po, _cell);
        }

        public void Redo() => Execute();
    }


    // ── Single-object transform ───────────────────────────────────────────────

    /// <summary>
    /// A completed gizmo drag on a single object (translate, rotate, or scale).
    /// For multi-object drags, use GroupTransformCommand instead. (NOT WORKING YET — TODO)
    /// </summary>
    public class TransformCommand : ICommand
    {
        public string Label { get; }

        private readonly Transform  _target;
        private readonly Vector3    _posBefore,   _posAfter;
        private readonly Quaternion _rotBefore,   _rotAfter;
        private readonly Vector3    _scaleBefore, _scaleAfter;

        public TransformCommand(Transform target,
            Vector3 posBefore, Quaternion rotBefore, Vector3 scaleBefore,
            Vector3 posAfter,  Quaternion rotAfter,  Vector3 scaleAfter,
            string label)
        {
            _target      = target;
            _posBefore   = posBefore; _posAfter   = posAfter;
            _rotBefore   = rotBefore; _rotAfter   = rotAfter;
            _scaleBefore = scaleBefore; _scaleAfter = scaleAfter;
            Label        = label;
        }

        // The drag already completed before Record() is called.
        public void Execute() { }
        public void Undo()    => Apply(_posBefore, _rotBefore, _scaleBefore);
        public void Redo()    => Apply(_posAfter,  _rotAfter,  _scaleAfter);

        private void Apply(Vector3 pos, Quaternion rot, Vector3 sca)
        {
            if (_target == null) return;
            _target.SetPositionAndRotation(pos, rot);
            _target.localScale = sca;
        }
    }


    // ── Group transform ───────────────────────────────────────────────────────

    /// <summary>
    /// Completed gizmo drag on a multi-object selection.
    /// One undo step restores every object in the group simultaneously.
    /// (NOT WORKING YET — TODO)
    /// </summary>
    public class GroupTransformCommand : ICommand
    {
        public string Label { get; }

        private readonly Transform[]  _targets;
        private readonly Vector3[]    _posB, _posA;
        private readonly Quaternion[] _rotB, _rotA;
        private readonly Vector3[]    _scaB, _scaA;

        /// <param name="posBefore">Positions at drag start — captured before the drag.</param>
        public GroupTransformCommand(
            IReadOnlyList<DecorObject> group,
            Vector3[]    posBefore,
            Quaternion[] rotBefore,
            Vector3[]    scaleBefore,
            string       label)
        {
            int n    = group.Count;
            _targets = new Transform[n];
            _posB    = new Vector3[n];    _posA = new Vector3[n];
            _rotB    = new Quaternion[n]; _rotA = new Quaternion[n];
            _scaB    = new Vector3[n];    _scaA = new Vector3[n];

            for (int i = 0; i < n; i++)
            {
                _targets[i] = group[i].transform;
                _posB[i]    = posBefore[i];
                _rotB[i]    = rotBefore[i];
                _scaB[i]    = scaleBefore[i];
                _posA[i]    = _targets[i].position;
                _rotA[i]    = _targets[i].rotation;
                _scaA[i]    = _targets[i].localScale;
            }

            Label = label;
        }

        public void Execute() { }
        public void Undo()    => Apply(_posB, _rotB, _scaB);
        public void Redo()    => Apply(_posA, _rotA, _scaA);

        private void Apply(Vector3[] pos, Quaternion[] rot, Vector3[] sca)
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                if (_targets[i] == null) continue;
                _targets[i].SetPositionAndRotation(pos[i], rot[i]);
                _targets[i].localScale = sca[i];
            }
        }
    }


    // ── Terrain regeneration ─────────────────────────────────────────────────

    /// <summary>
    /// Terrain rebuild with new dimensions or color.
    /// </summary>
    public class TerrainCommand : ICommand
    {
        public string Label => "Regenerate terrain";

        private readonly TerrainBuilder _tb;
        private readonly int   _wB, _dB, _thB; private readonly float _yB; private readonly Color _cB;
        private readonly int   _wA, _dA, _thA; private readonly float _yA; private readonly Color _cA;

        public TerrainCommand(TerrainBuilder tb,
            int wB, int dB, int thB, float yB, Color cB,   // before
            int wA, int dA, int thA, float yA, Color cA)   // after
        {
            _tb = tb;
            _wB = wB; _dB = dB; _thB = thB; _yB = yB; _cB = cB;
            _wA = wA; _dA = dA; _thA = thA; _yA = yA; _cA = cA;
        }

        public void Execute() => Apply(_wA, _dA, _thA, _yA, _cA);
        public void Undo()    => Apply(_wB, _dB, _thB, _yB, _cB);
        public void Redo()    => Execute();

        private void Apply(int w, int d, int th, float y, Color c)
        {
            if (_tb == null) return;
            _tb.width = w; _tb.depth = d;
            _tb.thickness = th; _tb.baseHeight = y;
            _tb.terrainColor = c;
            _tb.GenerateTerrain();
        }
    }
}
