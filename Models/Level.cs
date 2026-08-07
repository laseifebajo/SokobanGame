namespace SokobanGame.Models
{
    public class Level
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Grid { get; set; } = string.Empty;
        public bool IsBuiltIn { get; set; } = true;
    }
}