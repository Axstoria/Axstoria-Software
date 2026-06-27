using System.Collections.Generic;

namespace Shared.Domain
{
    public interface IHasTags
    {
        HashSet<string> Tags { get; }
    }
}
