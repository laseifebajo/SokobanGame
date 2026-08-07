using SokobanGame.Models;
using SokobanGame.Services;
using SokobanGame.ViewModels;

namespace SokobanGame.Views
{
    // This page is responsible for showing the actual Sokoban game.
    [QueryProperty(nameof(Level), "Level")]
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly PersistenceService _persistence;

        // The selected level is passed to this page when the player chooses a level.
        public Level? Level
        {
            set
            {
                if (value != null)
                {
                    // Give the selected level to the ViewModel so the game can start.
                    _viewModel.LoadLevel(value);

                    // Show the level name at the top of the page.
                    LevelNameLabel.Text = value.Name;

                    // Draw the level on the screen.
                    RenderGrid();
                }
            }
        }

        public GamePage(GameViewModel viewModel, PersistenceService persistence)
        {
            InitializeComponent();

            // The ViewModel handles the game logic and the persistence service
            // is used for saving the player's progress.
            _viewModel = viewModel;
            _persistence = persistence;

            // Set up the swipe controls so the player can move using gestures.
            SetupSwipeControls();
        }

        private void SetupSwipeControls()
        {
            // Create four swipe controls, one for each direction the player can move.
            var swipeUp    = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            var swipeDown  = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            var swipeLeft  = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };

            // When the player swipes, call MovePlayer with the matching direction.
            swipeUp.Swiped    += (s, e) => MovePlayer(GameEngine.Direction.Up);
            swipeDown.Swiped  += (s, e) => MovePlayer(GameEngine.Direction.Down);
            swipeLeft.Swiped  += (s, e) => MovePlayer(GameEngine.Direction.Left);
            swipeRight.Swiped += (s, e) => MovePlayer(GameEngine.Direction.Right);

            // Add the swipe controls to the game grid so they can detect the gestures.
            GameGrid.GestureRecognizers.Add(swipeUp);
            GameGrid.GestureRecognizers.Add(swipeDown);
            GameGrid.GestureRecognizers.Add(swipeLeft);
            GameGrid.GestureRecognizers.Add(swipeRight);
        }

        private async void MovePlayer(GameEngine.Direction dir)
        {
            // Ask the ViewModel to move the player and check if the level has been solved.
            bool won = await _viewModel.MoveAsync(dir);

            // Update the number of moves shown on screen and redraw the level.
            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();

            if (won)
            {
                // Save the player's result once the level has been completed.
                await SaveProgress();

                bool next = await DisplayAlertAsync("Level Complete! 🎉",
                    $"You solved it in {_viewModel.MoveCount} moves!",
                    "Next Level", "Back to Menu");

                // Return to the previous page after completing the level.
                // Both choices currently go back to the level selection screen.
                if (next)
                    await Shell.Current.GoToAsync("..");
                else
                    await Shell.Current.GoToAsync("..");
            }
        }

        private async Task SaveProgress()
        {
            // There is nothing to save if a level has not been loaded.
            if (_viewModel.CurrentLevel == null) return;

            // Load the progress that has already been saved.
            var progress = await _persistence.LoadProgressAsync() ?? new List<LevelProgress>();

            // Check if this level already has a saved result.
            var existing = progress.FirstOrDefault(
                p => p.LevelId == _viewModel.CurrentLevel.Id);

            if (existing == null)
            {
                // If this is the first time completing the level,
                // create a new progress record for it.
                progress.Add(new LevelProgress
                {
                    LevelId = _viewModel.CurrentLevel.Id,
                    IsCompleted = true,
                    BestMoves = _viewModel.MoveCount
                });
            }
            else
            {
                // The level has already been completed, so update its result.
                existing.IsCompleted = true;

                // Only replace the best score if the player has used fewer moves.
                if (_viewModel.MoveCount < existing.BestMoves)
                    existing.BestMoves = _viewModel.MoveCount;
            }

            // Save the updated progress so it is remembered when the app is reopened.
            await _persistence.SaveProgressAsync(progress);
        }

        private void RenderGrid()
        {
            // Don't try to draw anything if there is no level loaded.
            if (_viewModel.Grid == null) return;

            // Clear the old grid before drawing the updated version.
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            int rows = _viewModel.Grid.GetLength(0);
            int cols = _viewModel.Grid.GetLength(1);

            // Work out a cell size that fits on the device screen.
            // The maximum size of a cell is 55 so larger levels still fit.
            int cellSize = Math.Min(
                (int)(DeviceDisplay.Current.MainDisplayInfo.Width / cols /
                DeviceDisplay.Current.MainDisplayInfo.Density),
                55);

            // Create the correct number of rows and columns for the level.
            for (int r = 0; r < rows; r++)
                GameGrid.RowDefinitions.Add(new RowDefinition(cellSize));

            for (int c = 0; c < cols; c++)
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition(cellSize));

            // Go through every position in the level and create the matching visual cell.
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = CreateCell(_viewModel.Grid[r, c]);

                    // Put the cell in the correct row and column.
                    Grid.SetRow(cell, r);
                    Grid.SetColumn(cell, c);

                    GameGrid.Children.Add(cell);
                }
            }
        }

        // Changes each character in the level into something that can be displayed.
        // For example, # is a wall, $ is a box and @ is the player.
        private View CreateCell(char symbol) => symbol switch
        {
            '#' => new BoxView { Color = Colors.DarkSlateGray },
            '@' => new Label
            {
                Text = "🧍",
                FontSize = 22,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            },
            '$' => new BoxView
            {
                Color = Colors.SaddleBrown,
                Margin = 3,
                CornerRadius = 4
            },
            '.' => new BoxView
            {
                Color = Colors.LightBlue,
                Margin = 6,
                CornerRadius = 20
            },
            '*' => new BoxView
            {
                Color = Colors.Green,
                Margin = 3,
                CornerRadius = 4
            },
            '+' => new Label
            {
                Text = "🧍",
                FontSize = 22,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Colors.LightBlue
            },
            _ => new BoxView { Color = Color.FromArgb("#e8e8e8") }
        };

        private void OnBackClicked(object sender, EventArgs e)
            // Go back to the previous page when the back button is clicked.
            => Shell.Current.GoToAsync("..");

        private void OnUndoClicked(object sender, EventArgs e)
        {
            // Undo the player's last move, then update the screen.
            _viewModel.Undo();
            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();
        }

        private void OnResetClicked(object sender, EventArgs e)
        {
            // Reset the level back to its starting position.
            _viewModel.Reset();

            // Reset the move counter and redraw the level.
            MoveCountLabel.Text = "Moves: 0";
            RenderGrid();
        }
    }
}