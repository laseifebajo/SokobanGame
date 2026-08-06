using System.ComponentModel;
using System.Runtime.CompilerServices;
using SokobanGame.Models;
using SokobanGame.Services;

namespace SokobanGame.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        // Handles the actual game movement logic
        private readonly GameEngine _engine = new();

        // Used to play game sounds
        private readonly SoundService _sound;

        // Stores previous grids for undo
        private Stack<char[,]> _undoHistory = new();

        private char[,]? _grid;
        private int _moveCount;

        public Level? CurrentLevel { get; private set; }

        // The current game grid
        public char[,]? Grid
        {
            get => _grid;
            set
            {
                _grid = value;
                OnPropertyChanged();
            }
        }

        // Keeps track of how many moves the player has made
        public int MoveCount
        {
            get => _moveCount;
            set
            {
                _moveCount = value;
                OnPropertyChanged();
            }
        }

        public GameViewModel(SoundService sound)
        {
            _sound = sound;
        }

        // Loads a level and resets the game
        public void LoadLevel(Level level)
        {
            CurrentLevel = level;

            // Convert the level string into a grid
            _grid = _engine.ParseLevel(level.Grid);

            MoveCount = 0;
            _undoHistory.Clear();
        }

        // Tries to move the player in the selected direction
        public async Task<bool> MoveAsync(GameEngine.Direction dir)
        {
            if (_grid == null)
                return false;

            // Save the current grid in case the player wants to undo
            _undoHistory.Push(_engine.CopyGrid(_grid));

            bool moved = _engine.TryMove(_grid, dir);

            if (moved)
            {
                MoveCount++;

                // Play movement sound
                await _sound.PlayMoveAsync();

                OnPropertyChanged(nameof(Grid));

                // Check if the level has been completed
                if (_engine.IsComplete(_grid))
                    await _sound.PlayWinAsync();
            }
            else
            {
                // Remove saved grid because nothing moved
                _undoHistory.Pop();
            }

            return moved && _engine.IsComplete(_grid);
        }

        // Goes back to the previous move
        public void Undo()
        {
            if (_undoHistory.Count == 0 || _grid == null)
                return;

            _grid = _undoHistory.Pop();

            // Reduce move count when undoing
            MoveCount = Math.Max(0, MoveCount - 1);

            OnPropertyChanged(nameof(Grid));
        }

        // Restarts the current level
        public void Reset()
        {
            if (CurrentLevel != null)
                LoadLevel(CurrentLevel);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        // Updates the UI when a value changes
        protected void OnPropertyChanged(
            [CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(name));
        }
    }
}