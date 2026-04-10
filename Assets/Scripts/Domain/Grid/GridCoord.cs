namespace Domain
{
    /// <summary>
    /// Represents a coordinate in the grid system, defined by its X and Z values. This struct is used for indexing and referencing specific cells within the grid.
    /// </summary>
    public struct GridCoord
    {
        public int X { get; set; }
        public int Z { get; set; }

        public GridCoord(int x, int z) { X = x; Z = z; }

        public static GridCoord Zero => new(0, 0);

        public static GridCoord operator +(GridCoord a, GridCoord b) => new(a.X + b.X, a.Z + b.Z);

        public override string ToString() => $"({X}, {Z})";
    }
}
