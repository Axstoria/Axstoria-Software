using System.Collections.Generic;
using UnityEngine;
using VTT.Grid;

namespace VTT
{
    // ── Place ─────────────────────────────────────────────────────────────────

    /// <summary>Records placing a new DecorObject on the grid.</summary>
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

        public void Undo()
        {
            var ps = PlacementSystem.Instance;
            if (ps != null && _po != null) ps.Remove(_po);
            _instance.SetActive(false);
        }

        public void Redo()
        {
            _instance.SetActive(true);
            _instance.transform.SetPositionAndRotation(_worldPos, _rotation);
            _instance.transform.localScale = _scale;
            if (_parent != null) _instance.transform.SetParent(_parent, true);
            var ps = PlacementSystem.Instance;
            if (ps != null && _po != null) ps.Place(_po, _cell);
        }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>Records deleting a DecorObject (parks it inactive instead of destroying).</summary>
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
            _cell     = decor.gridCell;
            _worldPos = decor.transform.position;
            _rotation = decor.transform.rotation;
            _scale    = decor.transform.localScale;
            _parent   = decor.transform.parent;
            Label     = $"Delete {decor.displayName}";
        }

        public void Undo()
        {
            _instance.SetActive(true);
            _instance.transform.SetPositionAndRotation(_worldPos, _rotation);
            _instance.transform.localScale = _scale;
            if (_parent != null) _instance.transform.SetParent(_parent, true);
            var ps = PlacementSystem.Instance;
            if (ps != null && _po != null) ps.Place(_po, _cell);
        }

        public void Redo()
        {
            var ps = PlacementSystem.Instance;
            if (ps != null && _po != null) ps.Remove(_po);
            _instance.SetActive(false);
        }
    }

    // ── Transform ─────────────────────────────────────────────────────────────

    /// <summary>Records a completed gizmo drag (translate, rotate, or scale).</summary>
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

        public void Undo()
        {
            if (_target == null) return;
            _target.SetPositionAndRotation(_posBefore, _rotBefore);
            _target.localScale = _scaleBefore;
        }

        public void Redo()
        {
            if (_target == null) return;
            _target.SetPositionAndRotation(_posAfter, _rotAfter);
            _target.localScale = _scaleAfter;
        }
    }

    // ── Terrain ───────────────────────────────────────────────────────────────

    /// <summary>Records a terrain regeneration (size / color change).</summary>
    public class TerrainCommand : ICommand
    {
        public string Label => "Regenerate terrain";

        private readonly TerrainBuilder _tb;
        private readonly int   _wB, _dB, _thB; private readonly float _yB; private readonly Color _cB;
        private readonly int   _wA, _dA, _thA; private readonly float _yA; private readonly Color _cA;

        public TerrainCommand(TerrainBuilder tb,
            int wB, int dB, int thB, float yB, Color cB,
            int wA, int dA, int thA, float yA, Color cA)
        {
            _tb = tb;
            _wB = wB; _dB = dB; _thB = thB; _yB = yB; _cB = cB;
            _wA = wA; _dA = dA; _thA = thA; _yA = yA; _cA = cA;
        }

        public void Undo() => Apply(_wB, _dB, _thB, _yB, _cB);
        public void Redo() => Apply(_wA, _dA, _thA, _yA, _cA);

        private void Apply(int w, int d, int th, float y, Color c)
        {
            if (_tb == null) return;
            _tb.width = w; _tb.depth = d;
            _tb.thickness = th; _tb.baseHeight = y;
            _tb.terrainColor = c;
            _tb.GenerateTerrain();
        }
    }

    // ── Group Transform ───────────────────────────────────────────────────────

    /// <summary>
    /// Records a gizmo drag on a multi-object selection.
    /// Captures before/after for every object in one undo step.
    /// </summary>
    public class GroupTransformCommand : ICommand
    {
        public string Label { get; }

        private readonly Transform[]  _targets;
        private readonly Vector3[]    _posB, _posA;
        private readonly Quaternion[] _rotB, _rotA;
        private readonly Vector3[]    _scaB, _scaA;

        public GroupTransformCommand(
            IReadOnlyList<DecorObject> group,
            Vector3[]    posBefore,
            Quaternion[] rotBefore,
            Vector3[]    scaleBefore,
            string label)
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
                // Capture current (after) state at record time
                _posA[i]    = _targets[i].position;
                _rotA[i]    = _targets[i].rotation;
                _scaA[i]    = _targets[i].localScale;
            }

            Label = label;
        }

        public void Undo() => Apply(_posB, _rotB, _scaB);
        public void Redo() => Apply(_posA, _rotA, _scaA);

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

}
