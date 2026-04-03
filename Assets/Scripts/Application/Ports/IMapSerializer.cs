using Domain;

namespace App.Ports
{
    public interface IMapSerializer
    {
        string Serialize(Map map);
        Map Deserialize(string json);
    }
}
