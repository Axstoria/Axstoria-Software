using HexGrid.Models;
using UnityEngine;

namespace HexGrid.Persistence
{
    public class JsonMapSerializer : IMapSerializer
    {
        public string Serialize(MapDataDTO data, bool pretty = true)
            => JsonUtility.ToJson(data, pretty);

        public MapDataDTO Deserialize(string json)
            => JsonUtility.FromJson<MapDataDTO>(json);
    }
}
