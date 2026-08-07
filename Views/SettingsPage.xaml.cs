using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class SettingsPage : ContentPage
    {
        // These services are used to save settings and control the game sound.
        private readonly PersistenceService _persistence;
        private readonly SoundService _sound;

        // Stops the toggle and picker events from saving settings while they are being loaded.
        private bool _loading = false;

        public SettingsPage(PersistenceService persistence, SoundService sound)
        {
            InitializeComponent();

            // Get the services needed for this page.
            _persistence = persistence;
            _sound = sound;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Load the saved settings when the settings page is opened.
            _loading = true;
            var settings = await _persistence.LoadSettingsAsync();

            // Set the controls to match the saved settings.
            ThemeSwitch.IsToggled = settings.Theme == "Dark";
            SoundSwitch.IsToggled = settings.SoundEnabled;
            ColourPicker.SelectedItem = settings.GridColour;

            _loading = false;
        }

        private async void OnThemeToggled(object sender, ToggledEventArgs e)
        {
            // Don't save anything while the settings are being loaded.
            if (_loading) return;

            // Change the app theme depending on whether the switch is on or off.
            Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;

            // Load the current settings, update the theme and save them.
            var settings = await _persistence.LoadSettingsAsync();
            settings.Theme = e.Value ? "Dark" : "Light";
            await _persistence.SaveSettingsAsync(settings);
        }

        private async void OnSoundToggled(object sender, ToggledEventArgs e)
        {
            // Don't save anything while the settings are being loaded.
            if (_loading) return;

            // Turn the game sound on or off.
            _sound.SetEnabled(e.Value);

            // Save the new sound setting so it is remembered next time.
            var settings = await _persistence.LoadSettingsAsync();
            settings.SoundEnabled = e.Value;
            await _persistence.SaveSettingsAsync(settings);
        }

        private async void OnColourChanged(object sender, EventArgs e)
        {
            // Only save the colour when the user has selected one.
            if (_loading || ColourPicker.SelectedItem == null) return;

            // Save the selected grid colour.
            var settings = await _persistence.LoadSettingsAsync();
            settings.GridColour = ColourPicker.SelectedItem.ToString()!;
            await _persistence.SaveSettingsAsync(settings);
        }

        private async void OnBackClicked(object sender, EventArgs e)
            // Go back to the previous page.
            => await Shell.Current.GoToAsync("..");
    }
}