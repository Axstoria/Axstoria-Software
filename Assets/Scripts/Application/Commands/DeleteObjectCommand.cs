using System.Collections.Generic;
using Domain;

namespace App.Commands
{
    public class DeleteObjectCommand : ICommand
    {
        public string Label => $"Delete {_obj.DisplayName}";

        private readonly Map _map;
        private readonly Grid _grid;
        private readonly SceneObject _obj;
        private readonly GridCoord _origin;
        private readonly List<GridCoord> _footprint;

        public DeleteObjectCommand(Map map, Grid grid, SceneObject obj)
        {
            _map  = map;
            _grid = grid;
            _obj  = obj;
            // Capture placement data from the grid before removal
            _grid.TryGetPlacement(obj.Id, out _origin, out _footprint);
        }

        public void Execute()
        {
            _grid.RemoveOccupant(_obj.Id);
            _map.RemoveObject(_obj);
        }

        public void Undo()
        {
            _map.AddObject(_obj);
            _grid.PlaceOccupant(_obj.Id, _footprint, _origin);
        }

        public void Redo() => Execute();
    }
}
