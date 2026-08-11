using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class SettingsPage : ContentPage
    {
        private readonly PersistenceService _persistence;
        private readonly SoundService _sound;

        // Stops the switches from trying to save while we are just loading the settings
        private bool _loading = false;

        public SettingsPage(PersistenceService persistence, SoundService sound)
        {
            InitializeComponent();
            _persistence = persistence;
            _sound = sound;
        }

        // Load the saved settings every time this page is opened
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _loading = true;

            var settings = await _persistence.LoadSettingsAsync();

            // Set the controls to match what the user saved before
            ThemeSwitch.IsToggled = settings.Theme == "Dark";
            SoundSwitch.IsToggled = settings.SoundEnabled;
            ColourPicker.SelectedItem = settings.GridColour;

            // Apply the saved theme to the app
            Application.Current!.UserAppTheme = settings.Theme == "Dark"
                ? AppTheme.Dark
                : AppTheme.Light;

            _loading = false;
        }

        // Change the theme straight away and save the new setting
        private async void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            if (_loading) return;

            Application.Current!.UserAppTheme = e.Value
                ? AppTheme.Dark
                : AppTheme.Light;

            var settings = await _persistence.LoadSettingsAsync();
            settings.Theme = e.Value ? "Dark" : "Light";

            await _persistence.SaveSettingsAsync(settings);
        }

        // Turn the sound on or off and save the choice
        private async void OnSoundToggled(object sender, ToggledEventArgs e)
        {
            if (_loading) return;

            _sound.SetEnabled(e.Value);

            var settings = await _persistence.LoadSettingsAsync();
            settings.SoundEnabled = e.Value;

            await _persistence.SaveSettingsAsync(settings);
        }

        // Save the colour the user picked for the game grid
        private async void OnColourChanged(object sender, EventArgs e)
        {
            if (_loading || ColourPicker.SelectedItem == null) return;

            var settings = await _persistence.LoadSettingsAsync();
            settings.GridColour = ColourPicker.SelectedItem.ToString()!;

            await _persistence.SaveSettingsAsync(settings);
        }

        // Go back to the previous page
        private async void OnBackClicked(object sender, EventArgs e)
            => await Shell.Current.GoToAsync("..");
    }
}