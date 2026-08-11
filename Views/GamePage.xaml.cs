using Microsoft.Maui.Controls.Shapes;
using SokobanGame.Models;
using SokobanGame.Services;
using SokobanGame.ViewModels;

namespace SokobanGame.Views
{
    [QueryProperty(nameof(Level), "Level")]
    public partial class GamePage : ContentPage
    {
        private readonly GameViewModel _viewModel;
        private readonly PersistenceService _persistence;

        // The level is passed from the level select page when I choose a level.
        public Level? Level
        {
            set
            {
                if (value != null)
                {
                    // I load the selected level into the ViewModel so the game
                    // knows which level and grid the player is working with.
                    _viewModel.LoadLevel(value);

                    // I show the level name so the player knows which level they are on.
                    LevelNameLabel.Text = value.Name;

                    // Draw the level after loading it so it appears on screen.
                    RenderGrid();
                }
            }
        }

        public GamePage(GameViewModel viewModel, PersistenceService persistence)
        {
            InitializeComponent();

            // I use the ViewModel for the game logic instead of putting
            // all the movement code directly in this page.
            _viewModel = viewModel;

            // I need the persistence service here so I can save completed levels.
            _persistence = persistence;

            // Set up swiping so the player can control the game on mobile.
            SetupSwipeControls();
        }

        // I use swipe gestures because the game is designed to work on mobile.
        private void SetupSwipeControls()
        {
            var swipeUp = new SwipeGestureRecognizer { Direction = SwipeDirection.Up };
            var swipeDown = new SwipeGestureRecognizer { Direction = SwipeDirection.Down };
            var swipeLeft = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
            var swipeRight = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };

            // Each swipe calls MovePlayer with the matching direction.
            swipeUp.Swiped += (s, e) => MovePlayer(GameEngine.Direction.Up);
            swipeDown.Swiped += (s, e) => MovePlayer(GameEngine.Direction.Down);
            swipeLeft.Swiped += (s, e) => MovePlayer(GameEngine.Direction.Left);
            swipeRight.Swiped += (s, e) => MovePlayer(GameEngine.Direction.Right);

            // Add all four gestures to the game grid so it can detect the swipes.
            GameGrid.GestureRecognizers.Add(swipeUp);
            GameGrid.GestureRecognizers.Add(swipeDown);
            GameGrid.GestureRecognizers.Add(swipeLeft);
            GameGrid.GestureRecognizers.Add(swipeRight);
        }

        // This runs when the player tries to move.
        private async void MovePlayer(GameEngine.Direction dir)
        {
            // The ViewModel checks if the move is valid and updates the game.
            bool won = await _viewModel.MoveAsync(dir);

            // Update the move counter and redraw the grid to show the new position.
            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();

            // MoveAsync tells me if the player has completed the level.
            if (won)
            {
                await SaveProgress();

                // Give the player the choice to leave or play the level again.
                bool goBack = await DisplayAlertAsync(
                    "Level Complete! 🎉",
                    $"You solved it in {_viewModel.MoveCount} moves!",
                    "Back to Levels", "Play Again");

                if (goBack)
                    await Shell.Current.GoToAsync("..");
                else
                {
                    // If they choose to play again, reset everything back to the start.
                    _viewModel.Reset();
                    MoveCountLabel.Text = "Moves: 0";
                    RenderGrid();
                }
            }
        }

        // I save the level as completed and keep track of the best score.
        private async Task SaveProgress()
        {
            // If there is no level loaded, there is nothing to save.
            if (_viewModel.CurrentLevel == null) return;

            // Load any progress that was already saved.
            // If there is none, start with an empty list.
            var progress = await _persistence.LoadProgressAsync()
                ?? new List<LevelProgress>();

            // Look for a previous result for the current level.
            var existing = progress.FirstOrDefault(
                p => p.LevelId == _viewModel.CurrentLevel.Id);

            if (existing == null)
            {
                // If this is the first completion, create a new progress record.
                progress.Add(new LevelProgress
                {
                    LevelId = _viewModel.CurrentLevel.Id,
                    IsCompleted = true,
                    BestMoves = _viewModel.MoveCount
                });
            }
            else
            {
                // The level was already completed, so keep it marked as completed.
                existing.IsCompleted = true;

                // Only replace the old score if the new attempt used fewer moves.
                if (_viewModel.MoveCount < existing.BestMoves)
                    existing.BestMoves = _viewModel.MoveCount;
            }

            // Save the updated progress so it is not lost when the app closes.
            await _persistence.SaveProgressAsync(progress);
        }

        // This takes the 2D character array from the ViewModel
        // and turns it into the visible game board.
        private void RenderGrid()
        {
            if (_viewModel.Grid == null) return;

            // Clear the old grid first because I need to redraw it
            // after the player moves.
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            int rows = _viewModel.Grid.GetLength(0);
            int cols = _viewModel.Grid.GetLength(1);

            // Work out the available screen size so the game board
            // can fit properly on different screen sizes.
            double screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width
                / DeviceDisplay.Current.MainDisplayInfo.Density;
            double screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height
                / DeviceDisplay.Current.MainDisplayInfo.Density;

            // I calculate the cell size based on both the width and height.
            // This stops the grid from becoming too large in one direction.
            int cellByWidth = (int)((screenWidth - 20) / cols);
            int cellByHeight = (int)((screenHeight - 180) / rows);
            int cellSize = Math.Min(cellByWidth, cellByHeight);

            // I added a minimum size so the game is still easy to see and use.
            cellSize = Math.Max(cellSize, 35);

            // Create the rows and columns using the calculated cell size.
            for (int r = 0; r < rows; r++)
                GameGrid.RowDefinitions.Add(new RowDefinition(cellSize));

            for (int c = 0; c < cols; c++)
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition(cellSize));

            // Go through every position in the 2D array and create
            // the correct visual cell for that character.
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var cell = CreateCell(_viewModel.Grid[r, c], cellSize);

                    Grid.SetRow(cell, r);
                    Grid.SetColumn(cell, c);

                    GameGrid.Children.Add(cell);
                }
            }
        }

        // I pass the cell size into this method so the emojis and other
        // objects can scale depending on how big the game grid is.
        private View CreateCell(char symbol, int cellSize)
        {
            // Most cells have a floor underneath them.
            var floor = new BoxView
            {
                Color = Color.FromArgb("#c4a882")
            };

            // Choose what to display based on the character in the level array.
            View overlay = symbol switch
            {
                // A wall is just a darker block.
                '#' => new BoxView
                {
                    Color = Color.FromArgb("#7a7a8a")
                },

                // A box has a border and a symbol in the middle
                // to make it look different from the floor.
                '$' => new Border
                {
                    BackgroundColor = Color.FromArgb("#c8892a"),
                    Stroke = Color.FromArgb("#8b5e1a"),
                    StrokeThickness = 3,
                    StrokeShape = new RoundRectangle { CornerRadius = 4 },
                    Margin = 2,
                    Content = new Label
                    {
                        Text = "▦",
                        FontSize = cellSize * 0.45,
                        TextColor = Color.FromArgb("#8b5e1a"),
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                },

                // A target is shown as a green outlined square.
                '.' => new Border
                {
                    BackgroundColor = Colors.Transparent,
                    Stroke = Color.FromArgb("#2ecc71"),
                    StrokeThickness = 3,
                    StrokeShape = new RoundRectangle { CornerRadius = 4 },
                    Margin = 4,
                    Content = new Label
                    {
                        Text = "✕",
                        FontSize = cellSize * 0.45,
                        TextColor = Color.FromArgb("#2ecc71"),
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                },

                // A box on a target is green so the player can see
                // that they have correctly placed a box.
                '*' => new Border
                {
                    BackgroundColor = Color.FromArgb("#27ae60"),
                    Stroke = Color.FromArgb("#1e8449"),
                    StrokeThickness = 3,
                    StrokeShape = new RoundRectangle { CornerRadius = 4 },
                    Margin = 2,
                    Content = new Label
                    {
                        Text = "✕",
                        FontSize = cellSize * 0.45,
                        TextColor = Colors.White,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center
                    }
                },

                // The player is represented using an emoji.
                '@' => new Label
                {
                    Text = "🧍",
                    FontSize = cellSize * 0.6,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },

                // This is also the player, but the character is on a target.
                '+' => new Label
                {
                    Text = "🧍",
                    FontSize = cellSize * 0.6,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },

                // Anything else is treated as an empty floor cell.
                _ => new BoxView
                {
                    Color = Colors.Transparent
                }
            };

            // Walls do not need a floor underneath them.
            if (symbol == '#')
                return overlay;

            // For the other cells, put the object on top of the floor.
            var grid = new Grid();
            grid.Children.Add(floor);
            grid.Children.Add(overlay);

            return grid;
        }

        // Go back to the previous page when the back button is pressed.
        private void OnBackClicked(object sender, EventArgs e)
            => Shell.Current.GoToAsync("..");

        // Undo the last move using the move history stored in the ViewModel.
        private void OnUndoClicked(object sender, EventArgs e)
        {
            _viewModel.Undo();

            // Update the counter and redraw the board after undoing.
            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();
        }

        // Reset the level back to its original starting position.
        private void OnResetClicked(object sender, EventArgs e)
        {
            _viewModel.Reset();

            // Reset the counter and redraw the starting board.
            MoveCountLabel.Text = "Moves: 0";
            RenderGrid();
        }
    }
}

