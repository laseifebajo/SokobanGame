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
                // Open the level selection page when the player chooses Play.
                await Shell.Current.GoToAsync("LevelSelectPage");
            }
            catch (Exception ex)
            {
                // Show an error message if the page cannot be opened.
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnEditorClicked(object sender, EventArgs e)
        {
            try
            {
                // Open the level editor so the player can create their own levels.
                await Shell.Current.GoToAsync("LevelEditorPage");
            }
            catch (Exception ex)
            {
                // Tell the user if there is a problem opening the editor.
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                // Open the settings page when the settings button is clicked.
                await Shell.Current.GoToAsync("SettingsPage");
            }
            catch (Exception ex)
            {
                // Display the error instead of letting the app fail silently.
                await DisplayAlertAsync("Navigation Error", ex.Message, "OK");
            }
        }
    }
}