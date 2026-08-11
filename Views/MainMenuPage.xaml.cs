namespace SokobanGame.Views
{
    public partial class MainMenuPage : ContentPage
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private async void OnPlayClicked(object sender, EventArgs e)
        {
            try
            {
                // Take the player to the page where they can pick a level
                await Shell.Current.GoToAsync("LevelSelectPage");
            }
            catch (Exception ex)
            {
                // Show an error if something goes wrong with opening the page
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnEditorClicked(object sender, EventArgs e)
        {
            try
            {
                // Open the editor so the player can make their own level
                await Shell.Current.GoToAsync("LevelEditorPage");
            }
            catch (Exception ex)
            {
                // Let the player know if the editor could not be opened
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                // Open the settings page
                await Shell.Current.GoToAsync("SettingsPage");
            }
            catch (Exception ex)
            {
                // Show the error instead of the app failing without explaining why
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }
    }
}