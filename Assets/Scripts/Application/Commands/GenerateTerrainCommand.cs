using Domain;

namespace App.Commands
{
    /// <summary>
    /// Command to regenerate the terrain layout with new parameters. Implements the ICommand interface.
    /// </summary>
    public class GenerateTerrainCommand : ICommand
    {
        public string Label => "Regenerate terrain";

        private readonly TerrainLayout _terrain;
        private readonly Grid          _grid;
        private readonly Map           _map;
        private readonly int   _widthBefore,  _widthAfter;
        private readonly int   _depthBefore,  _depthAfter;
        private readonly int   _thickBefore,  _thickAfter;
        private readonly float _heightBefore, _heightAfter;
        private readonly float[] _colorBefore, _colorAfter;

        public GenerateTerrainCommand(TerrainLayout terrain, Grid grid, Map map,
            int widthAfter, int depthAfter, int thickAfter, float heightAfter, float[] colorAfter)
        {
            _terrain      = terrain;
            _grid         = grid;
            _map          = map;
            _widthBefore  = terrain.Width;
            _depthBefore  = terrain.Depth;
            _thickBefore  = terrain.Thickness;
            _heightBefore = terrain.Height;
            _colorBefore  = terrain.Color;
            _widthAfter   = widthAfter;
            _depthAfter   = depthAfter;
            _thickAfter   = thickAfter;
            _heightAfter  = heightAfter;
            _colorAfter   = colorAfter;
        }

        public void Execute() => Apply(_widthAfter,  _depthAfter,  _thickAfter,  _heightAfter,  _colorAfter);
        public void Undo()    => Apply(_widthBefore, _depthBefore, _thickBefore, _heightBefore, _colorBefore);
        public void Redo()    => Execute();

        private void Apply(int width, int depth, int thickness, float height, float[] color)
        {
            _terrain.Width     = width;
            _terrain.Depth     = depth;
            _terrain.Thickness = thickness;
            _terrain.Height    = height;
            _terrain.Color     = color;

            // Rebuild occupancy from current object positions — orphaned occupants
            // outside the new bounds are cleared, objects inside stay registered.
            if (_grid != null && _map != null)
                _grid.RebuildOccupancy(_map.Objects);
        }
    }
}
