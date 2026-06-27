using GameSession.Domain;
using Grid.Domain;
using MapEditor.Domain;
using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class DeleteObjectUseCase
    {
        private readonly Map                _map;
        private readonly Grid.Domain.Grid   _grid;
        private readonly CommandHistory     _history;
        private readonly ISessionContext    _session;
        private readonly IPermissionService _permissions;

        public DeleteObjectUseCase(Map map, Grid.Domain.Grid grid, CommandHistory history, ISessionContext session, IPermissionService permissions)
        {
            _map         = map;
            _grid        = grid;
            _history     = history;
            _session     = session;
            _permissions = permissions;
        }

        public void Execute(SceneObject obj)
        {
            var player = _session.CurrentPlayer;
            if (!player.IsGameMaster && !_permissions.HasAccess(player, obj)) return;

            _history.Record(new DeleteObjectCommand(_map, _grid, obj));
        }
    }
}
