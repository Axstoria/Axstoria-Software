using System;
using System.Collections.Generic;

namespace Domain
{
    public class Map
    {
        public string Id   { get; set; }
        public string Name { get; set; }

        public List<Token>       Tokens        { get; set; } = new();
        public List<Structure>   Structures    { get; set; } = new();
        public List<SceneObject> Objects       { get; set; } = new();
        public TerrainLayout     TerrainLayout { get; set; }

        public event Action<SceneObject> OnObjectAdded;
        public event Action<SceneObject> OnObjectRemoved;

        public void AddObject(SceneObject obj)
        {
            Objects.Add(obj);
            OnObjectAdded?.Invoke(obj);
        }

        public void RemoveObject(SceneObject obj)
        {
            Objects.Remove(obj);
            OnObjectRemoved?.Invoke(obj);
        }
    }
}
