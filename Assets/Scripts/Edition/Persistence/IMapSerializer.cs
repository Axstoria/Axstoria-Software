using Edition.Models;

namespace Edition.Persistence
{
    public interface IMapSerializer
    {
        string Serialize(MapDataDTO data, bool pretty = true);
        MapDataDTO Deserialize(string json);
    }
}
