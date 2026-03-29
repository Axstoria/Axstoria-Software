using System.Collections.Generic;
using UnityEngine;

namespace VTT.Grid
{
    /// <summary>
    /// A* pathfinder that operates on GridCells.
    /// Usage:  var path = AStarPathfinder.FindPath(startCell, goalCell);
    /// Returns a list of world-space Vector3 waypoints, or null if no path.
    /// </summary>
    public static class AStarPathfinder
    {
        private class Node
        {
            public GridCell Cell;
            public Node     Parent;
            public float    G;   // cost from start
            public float    H;   // heuristic to goal
            public float    F => G + H;

            public Node(GridCell cell, Node parent, float g, float h)
            {
                Cell = cell; Parent = parent; G = g; H = h;
            }
        }

        /// <summary>
        /// Find a path between two cells.
        /// Returns world-space waypoints (cell centres), or null if unreachable.
        /// </summary>
        public static List<Vector3> FindPath(GridCell start, GridCell goal, bool allowDiagonals = true)
        {
            if (start == null || goal == null || !goal.IsWalkable) return null;

            GridManager gm = GridManager.Instance;

            var open   = new List<Node>();
            var closed = new HashSet<GridCell>();

            open.Add(new Node(start, null, 0f, Heuristic(start, goal)));

            while (open.Count > 0)
            {
                // Pick lowest F
                Node current = open[0];
                for (int i = 1; i < open.Count; i++)
                    if (open[i].F < current.F) current = open[i];

                open.Remove(current);
                closed.Add(current.Cell);

                if (current.Cell == goal)
                    return BuildPath(current, gm);

                foreach (GridCell neighbour in gm.GetNeighbours(current.Cell, allowDiagonals))
                {
                    if (closed.Contains(neighbour)) continue;

                    bool  diagonal = neighbour.X != current.Cell.X && neighbour.Z != current.Cell.Z;
                    float stepCost = (diagonal ? 1.414f : 1f) * neighbour.MovementCost;
                    float newG     = current.G + stepCost;

                    Node existing = open.Find(n => n.Cell == neighbour);
                    if (existing == null)
                    {
                        open.Add(new Node(neighbour, current, newG, Heuristic(neighbour, goal)));
                    }
                    else if (newG < existing.G)
                    {
                        existing.G      = newG;
                        existing.Parent = current;
                    }
                }
            }

            return null; // no path found
        }

        /// <summary>Convenience overload: takes world positions.</summary>
        public static List<Vector3> FindPath(Vector3 startWorld, Vector3 goalWorld, bool allowDiagonals = true)
        {
            GridManager gm = GridManager.Instance;
            if (gm == null) return null;

            Vector2Int s = gm.WorldToGrid(startWorld);
            Vector2Int g = gm.WorldToGrid(goalWorld);
            return FindPath(gm.GetCell(s.x, s.y), gm.GetCell(g.x, g.y), allowDiagonals);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static float Heuristic(GridCell a, GridCell b)
        {
            // Octile distance — accurate for 8-directional movement
            float dx = Mathf.Abs(a.X - b.X);
            float dz = Mathf.Abs(a.Z - b.Z);
            return Mathf.Max(dx, dz) + (1.414f - 1f) * Mathf.Min(dx, dz);
        }

        private static List<Vector3> BuildPath(Node endNode, GridManager gm)
        {
            var path = new List<Vector3>();
            Node n = endNode;
            while (n != null)
            {
                path.Add(gm.GridToWorld(n.Cell.X, n.Cell.Z));
                n = n.Parent;
            }
            path.Reverse();
            return path;
        }
    }
}
