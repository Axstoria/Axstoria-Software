using System.Collections.Generic;
using SceneEditor.Domain;

namespace MapEditor.Domain
{
    public class PermissionService : IPermissionService
    {
        public bool CanInteract(Player player, SceneObject obj)
        {
            if (player == null || obj == null) return false;
            if (player.IsGameMaster) return true;
            if (!string.IsNullOrEmpty(player.PawnId) && player.PawnId == obj.Id) return true;

            var playerTagIds = TagIdsOf(player);
            if (playerTagIds.Count == 0) return false;

            var objectTagIds = TagIdsOf(obj);
            return playerTagIds.Overlaps(objectTagIds);
        }

        private static HashSet<string> TagIdsOf(IHasMetadata target)
        {
            var ids = new HashSet<string>();
            if (target.Metadata == null) return ids;

            foreach (var entry in target.Metadata)
                if (entry.EntryValue is TagValue tag)
                    ids.Add(tag.Id);

            return ids;
        }
    }
}
