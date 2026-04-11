using System.Collections.Generic;
using App.Commands;
using Domain;

namespace App.UseCases
{
    /// <summary>
    /// Use case for placing a new SceneObject on the map at a specific location.
    /// </summary>
    public class PlaceObjectUseCase
    {
        private readonly Map _map;
        private readonly Grid _grid;
        private readonly CommandHistory _history;

        public PlaceObjectUseCase(Map map, Grid grid, CommandHistory history)
        {
            _map     = map;
            _grid    = grid;
            _history = history;
        }

        public bool CanPlace(List<GridCoord> footprint, GridCoord origin)
        {
            return _grid.CanPlace(footprint, origin);
        }

        // footprint is computed by the View from Unity renderer bounds and passed here
        public void Execute(SceneObject obj, GridCoord origin, List<GridCoord> footprint)
        {
            if (!CanPlace(footprint, origin)) return;
            _history.Record(new PlaceObjectCommand(_map, _grid, obj, origin, footprint));
        }
    }
}
