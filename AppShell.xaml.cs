namespace SokobanGame
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // These routes let the app move between the different pages
            Routing.RegisterRoute("LevelSelectPage", typeof(Views.LevelSelectPage));
            Routing.RegisterRoute("GamePage", typeof(Views.GamePage));
            Routing.RegisterRoute("LevelEditorPage", typeof(Views.LevelEditorPage));
            Routing.RegisterRoute("SettingsPage", typeof(Views.SettingsPage));
        }
    }
}