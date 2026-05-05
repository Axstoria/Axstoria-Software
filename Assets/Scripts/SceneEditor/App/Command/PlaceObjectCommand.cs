using System.Collections.Generic;
using Grid.Domain;
using MapEditor.Domain;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.Command
{
    public class PlaceObjectCommand : ICommand
    {
        public string Label => $"Place {_obj.DisplayName}";

        private readonly Map              _map;
        private readonly Grid.Domain.Grid _grid;
        private readonly SceneObject      _obj;
        private readonly GridCoord        _origin;
        private readonly List<GridCoord>  _footprint;

        public PlaceObjectCommand(Map map, Grid.Domain.Grid grid, SceneObject obj,
            GridCoord origin, List<GridCoord> footprint)
        {
            _map       = map;
            _grid      = grid;
            _obj       = obj;
            _origin    = origin;
            _footprint = footprint;
        }

        public void Execute()
        {
            _map.AddObject(_obj);
            _grid.PlaceOccupant(_obj.Id, _footprint, _origin);
        }

        public void Undo()
        {
            _grid.RemoveOccupant(_obj.Id);
            _map.RemoveObject(_obj);
        }

        public void Redo() => Execute();
    }
}
