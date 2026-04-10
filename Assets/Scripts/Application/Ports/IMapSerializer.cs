using Domain;

namespace App.Ports
{
    /// <summary>
    /// Interface for map serialization.
    /// </summary>
    public interface IMapSerializer
    {
        string Serialize(Map map);
        Map Deserialize(string json);
    }
}
