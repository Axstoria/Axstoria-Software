using UnityEngine;

namespace Domain
{
    using System.Collections.Generic;
    
    /// <summary>
    /// Represents a game map containing all the scene elements for a campaign level.
    /// A map is the core container that holds tokens, structures, scene objects, and terrain layouts.
    /// </summary>
    /// <remarks>
    /// The Map class serves as the primary data model for organizing all entities within a single game level.
    /// It manages collections of game objects that can be edited through the MVVM interface.
    /// </remarks>
    public class Map
    {
        /// <summary>
        /// Gets or sets the unique identifier for this map.
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the display name of the map.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the collection of tokens (characters/NPCs) on the map.
        /// </summary>
        /// <remarks>
        /// Tokens typically represent player characters, NPCs, or other interactive entities.
        /// </remarks>
        public List<Token> Tokens {get; set;}
        
        /// <summary>
        /// Gets or sets the collection of structures on the map.
        /// </summary>
        /// <remarks>
        /// Structures are typically buildings, walls, or other destructible/non-destructible environmental elements.
        /// </remarks>
        public List<Structure>  Structures {get; set;}
        
        /// <summary>
        /// Gets or sets the collection of interactive scene objects on the map.
        /// </summary>
        /// <remarks>
        /// Scene objects are interactable elements such as doors, levers, chests, or other props.
        /// </remarks>
        public List<SceneObject> Objects {get; set;}
        
        /// <summary>
        /// Gets or sets the collection of terrain layouts that define the map's geographical features.
        /// </summary>
        /// <remarks>
        /// Terrain layouts determine the walkable areas and environmental height maps.
        /// </remarks>
        public List<TerrainLayout> TerrainLayouts {get; set;}
    }
}