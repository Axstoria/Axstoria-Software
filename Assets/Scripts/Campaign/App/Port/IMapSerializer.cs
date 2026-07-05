using MapEditor.Domain;

namespace Campaign.App.Port
{
    public interface IMapSerializer
    {
        string Serialize(Map map);
        Map Deserialize(string json);
    }
}
