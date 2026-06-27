using GameSession.Domain;
using SceneEditor.App.Command;
using SceneEditor.Domain;
using Shared.Domain;

namespace SceneEditor.App.UseCase
{
    public class TransformObjectUseCase
    {
        private readonly CommandHistory   _history;
        private readonly ISessionContext  _session;
        private readonly IPermissionService _permissions;

        public TransformObjectUseCase(CommandHistory history, ISessionContext session, IPermissionService permissions)
        {
            _history     = history;
            _session     = session;
            _permissions = permissions;
        }

        public void Execute(SceneObject obj, TransformModel after, string label)
        {
            var player = _session.CurrentPlayer;
            if (!player.IsGameMaster && !_permissions.HasAccess(player, obj)) return;

            var before = obj.Transform;
            _history.Record(new TransformObjectCommand(obj, before, after, label));
        }
    }
}
