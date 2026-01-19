using HexGrid.Models;

namespace HexGrid.Persistence
{
    public interface IMapSerializer
    {
        string Serialize(MapDataDTO data, bool pretty = true);
        MapDataDTO Deserialize(string json);
    }
}
