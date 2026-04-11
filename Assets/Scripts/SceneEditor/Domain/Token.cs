namespace Domain
{
    /// <summary>
    /// Represents a character, NPC, or creature token on the map.
    /// Inherits from SceneModel and adds faction-related properties.
    /// </summary>
    /// <remarks>
    /// Tokens are the primary interactive entities in the game world, including player characters,
    /// enemies, NPCs, and other creatures that can move, act, and interact with the environment.
    /// </remarks>
    public class Token : SceneModel
    {
        /// <summary>
        /// Gets or sets the faction or allegiance this token belongs to.
        /// </summary>
        /// <remarks>
        /// Factions can be used to determine behavior, appearance, and relationships between entities.
        /// Examples: "player", "enemy", "neutral", or specific organization names.
        /// </remarks>
        public string Faction {get; set;}
    }
}