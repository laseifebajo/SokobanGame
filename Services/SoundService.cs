namespace SokobanGame.Services
{
    public class SoundService
    {
        private bool _enabled = true;

        public void SetEnabled(bool enabled) => _enabled = enabled;

        public Task PlayMoveAsync() => Task.CompletedTask;
        public Task PlayPushAsync() => Task.CompletedTask;
        public Task PlayWinAsync() => Task.CompletedTask;
    }
}