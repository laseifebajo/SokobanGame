namespace SokobanGame.Models
{
    public class LevelProgress
    {
        // Connects the progress to a specific level
        public required string LevelId { get; set; }

        // Checks if the player has completed the level
        public bool IsCompleted { get; set; }

        // Stores the player's lowest number of moves
        public int BestMoves { get; set; } = int.MaxValue;
    }
}