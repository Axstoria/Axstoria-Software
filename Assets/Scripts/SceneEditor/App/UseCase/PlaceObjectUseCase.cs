using System.Collections.Generic;
using GameSession.Domain;
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
        private readonly ISessionContext  _session;

        public PlaceObjectUseCase(Map map, Grid.Domain.Grid grid, CommandHistory history, ISessionContext session)
        {
            _map     = map;
            _grid    = grid;
            _history = history;
            _session = session;
        }

        public bool CanPlace(List<GridCoord> footprint, GridCoord origin)
        {
            return _grid.CanPlace(footprint, origin);
        }

        public void Execute(SceneObject obj, GridCoord origin, List<GridCoord> footprint)
        {
            if (!_session.CurrentPlayer.IsGameMaster) return;
            if (!CanPlace(footprint, origin)) return;
            _history.Record(new PlaceObjectCommand(_map, _grid, obj, origin, footprint));
        }
    }
}
