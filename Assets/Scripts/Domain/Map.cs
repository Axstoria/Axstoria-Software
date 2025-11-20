using UnityEngine;

namespace Domain
{
    using System.Collections.Generic;
    public class Map
    {
        public string Id { get; set; }
        public string Name { get; set; }
        
        public List<Token> Tokens {get; set;}
        public List<Structure>  Structures {get; set;}
        public List<SceneObject> Objects {get; set;}
        public List<TerrainLayout> TerrainLayouts {get; set;}
    }
}