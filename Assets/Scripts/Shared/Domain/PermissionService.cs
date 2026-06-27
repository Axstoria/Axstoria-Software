namespace Shared.Domain
{
    public class PermissionService : IPermissionService
    {
        public bool HasAccess(IHasTags principal, IHasTags resource)
            => principal.Tags.Overlaps(resource.Tags);
    }
}
