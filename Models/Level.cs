namespace SokobanGame.Models
{
    public class Level
    {
        // The unique ID of the level
        public required string Id { get; set; }

        // The name shown to the player
        public required string Name { get; set; }

        // Stores the level layout as a string
        public required string Grid { get; set; }

        // True if this level comes with the game
        public bool IsBuiltIn { get; set; } = true;
    }
}