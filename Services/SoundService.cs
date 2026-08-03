using Plugin.Maui.Audio;

namespace SokobanGame.Services
{
    public class SoundService
    {
        private bool _enabled = true;

        public void SetEnabled(bool enabled) => _enabled = enabled;

        public async Task PlayMoveAsync()
        {
            if (!_enabled) return;
            var player = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("move.mp3"));
            player.Play();
        }

        public async Task PlayPushAsync()
        {
            if (!_enabled) return;
            var player = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("push.mp3"));
            player.Play();
        }

        public async Task PlayWinAsync()
        {
            if (!_enabled) return;
            var player = Plugin.Maui.Audio.AudioManager.Current.CreatePlayer(
                await FileSystem.OpenAppPackageFileAsync("win.mp3"));
            player.Play();
        }
    }
}