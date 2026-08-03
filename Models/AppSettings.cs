namespace SokobanGame.Models
{
    public class AppSettings
    {
        //This string is used to store the user's  theme
        public string Theme { get; set; } = "Light";

        // this will store the chosen grid colour style
        public string GridColour { get; set; } = "Blue";

        // And here it Controls whether game sounds are enabled
        public bool SoundEnabled { get; set; } = true;
    }
}