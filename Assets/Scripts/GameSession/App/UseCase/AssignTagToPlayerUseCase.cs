using GameSession.Domain;

namespace GameSession.App.UseCase
{
    public class AssignTagToPlayerUseCase
    {
        private readonly ISessionContext _session;

        public AssignTagToPlayerUseCase(ISessionContext session)
        {
            _session = session;
        }

        public void Execute(Player player, string tagId, bool assign)
        {
            if (assign)
                player.Tags.Add(tagId);
            else
                player.Tags.Remove(tagId);
        }
    }
}
