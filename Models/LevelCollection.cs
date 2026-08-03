namespace SokobanGame.Models
{
    public class LevelCollection
    {
        // Holds all the levels loaded from the JSON file
        public List<Level> Levels { get; set; } = new();
    }
}