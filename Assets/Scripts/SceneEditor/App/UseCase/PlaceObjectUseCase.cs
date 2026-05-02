using System.Collections.Generic;
using Grid.Domain;
using MapEditor.Domain;
using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class PlaceObjectUseCase
    {
        private readonly Map              _map;
        private readonly Grid.Domain.Grid _grid;
        private readonly CommandHistory   _history;

        public PlaceObjectUseCase(Map map, Grid.Domain.Grid grid, CommandHistory history)
        {
            _map     = map;
            _grid    = grid;
            _history = history;
        }

        public bool CanPlace(List<GridCoord> footprint, GridCoord origin)
        {
            return _grid.CanPlace(footprint, origin);
        }

        public void Execute(SceneObject obj, GridCoord origin, List<GridCoord> footprint)
        {
            if (!CanPlace(footprint, origin)) return;
            _history.Record(new PlaceObjectCommand(_map, _grid, obj, origin, footprint));
        }
    }
}
