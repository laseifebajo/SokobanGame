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

        // The selected level gets passed over from the level select page
        public Level? Level
        {
            set
            {
                if (value != null)
                {
                    _viewModel.LoadLevel(value);
                    LevelNameLabel.Text = value.Name;
                    RenderGrid();
                }
            }
        }

        public GamePage(GameViewModel viewModel, PersistenceService persistence)
        {
            InitializeComponent();

            // I keep the game logic in the ViewModel so this page mainly deals with the UI
            _viewModel = viewModel;

            // Used to save the player's progress when they finish a level
            _persistence = persistence;

            // Set up swiping so the player can move by dragging their finger
            SetupPanGesture();
        }

        private void SetupPanGesture()
        {
            var pan = new PanGestureRecognizer();
            bool hasMoved = false;

            pan.PanUpdated += (s, e) =>
            {
                if (e.StatusType == GestureStatus.Started)
                {
                    hasMoved = false;
                }
                else if (e.StatusType == GestureStatus.Running && !hasMoved)
                {
                    double dx = e.TotalX;
                    double dy = e.TotalY;

                    // I use a small distance before moving so a tiny touch doesn't count as a move
                    if (Math.Abs(dx) > 30 || Math.Abs(dy) > 30)
                    {
                        // Stops one swipe from moving the player more than once
                        hasMoved = true;

                        // Whichever direction the finger moved the most is the direction used
                        if (Math.Abs(dx) > Math.Abs(dy))
                            MovePlayer(dx > 0
                                ? GameEngine.Direction.Right
                                : GameEngine.Direction.Left);
                        else
                            MovePlayer(dy > 0
                                ? GameEngine.Direction.Down
                                : GameEngine.Direction.Up);
                    }
                }
            };

            PageGrid.GestureRecognizers.Add(pan);
        }

        private void OnUpClicked(object sender, EventArgs e)
            => MovePlayer(GameEngine.Direction.Up);

        private void OnDownClicked(object sender, EventArgs e)
            => MovePlayer(GameEngine.Direction.Down);

        private void OnLeftClicked(object sender, EventArgs e)
            => MovePlayer(GameEngine.Direction.Left);

        private void OnRightClicked(object sender, EventArgs e)
            => MovePlayer(GameEngine.Direction.Right);

        private async void MovePlayer(GameEngine.Direction dir)
        {
            bool won = await _viewModel.MoveAsync(dir);

            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();

            if (won)
            {
                await SaveProgress();

                bool goBack = await DisplayAlertAsync(
                    "Level Complete! 🎉",
                    $"You solved it in {_viewModel.MoveCount} moves!",
                    "Back to Levels", "Play Again");

                if (goBack)
                    await Shell.Current.GoToAsync("..");
                else
                {
                    // Let the player try again if they want to beat their score
                    _viewModel.Reset();
                    MoveCountLabel.Text = "Moves: 0";
                    RenderGrid();
                }
            }
        }

        private async Task SaveProgress()
        {
            if (_viewModel.CurrentLevel == null) return;

            // Load the old scores first so completing one level doesn't overwrite the others
            var progress = await _persistence.LoadProgressAsync()
                ?? new List<LevelProgress>();

            var existing = progress.FirstOrDefault(
                p => p.LevelId == _viewModel.CurrentLevel.Id);

            if (existing == null)
            {
                // First time completing this level, so create a new score for it
                progress.Add(new LevelProgress
                {
                    LevelId = _viewModel.CurrentLevel.Id,
                    IsCompleted = true,
                    BestMoves = _viewModel.MoveCount
                });
            }
            else
            {
                existing.IsCompleted = true;

                // Lower moves is better, so only replace the old score if this attempt was better
                if (_viewModel.MoveCount < existing.BestMoves)
                    existing.BestMoves = _viewModel.MoveCount;
            }

            await _persistence.SaveProgressAsync(progress);
        }

        private void RenderGrid()
        {
            if (_viewModel.Grid == null) return;

            // Clear the old version because the board changes after every move
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            int rows = _viewModel.Grid.GetLength(0);
            int cols = _viewModel.Grid.GetLength(1);

            double screenWidth = DeviceDisplay.Current.MainDisplayInfo.Width
                / DeviceDisplay.Current.MainDisplayInfo.Density;

            double screenHeight = DeviceDisplay.Current.MainDisplayInfo.Height
                / DeviceDisplay.Current.MainDisplayInfo.Density;

            // Work out the cell size so different sized levels can still fit on the screen
            int cellByWidth = (int)((screenWidth - 20) / cols);
            int cellByHeight = (int)((screenHeight - 220) / rows);
            int cellSize = Math.Min(cellByWidth, cellByHeight);

            // Stop the cells becoming too small to see
            cellSize = Math.Max(cellSize, 35);

            for (int r = 0; r < rows; r++)
                GameGrid.RowDefinitions.Add(new RowDefinition(cellSize));

            for (int c = 0; c < cols; c++)
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition(cellSize));

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

        private View CreateCell(char symbol, int cellSize)
        {
            // Walls are different because they don't need a floor underneath
            if (symbol == '#')
            {
                return new Image
                {
                    Source = "wall.png",
                    Aspect = Aspect.Fill,
                    WidthRequest = cellSize,
                    HeightRequest = cellSize
                };
            }

            // Start with the ground, then put the player/box/target on top of it
            var grid = new Grid
            {
                WidthRequest = cellSize,
                HeightRequest = cellSize
            };

            grid.Children.Add(new Image
            {
                Source = "ground.png",
                Aspect = Aspect.Fill,
                WidthRequest = cellSize,
                HeightRequest = cellSize
            });

            string? spriteSource = symbol switch
            {
                '$' => "box.png",
                '*' => "box_on_target.png",
                '.' => "target.png",
                '@' => "player.png",
                '+' => "player.png",
                _   => null
            };

            if (spriteSource != null)
            {
                grid.Children.Add(new Image
                {
                    Source = spriteSource,
                    Aspect = Aspect.AspectFit,
                    WidthRequest = cellSize,
                    HeightRequest = cellSize
                });
            }

            return grid;
        }

        private void OnBackClicked(object sender, EventArgs e)
            => Shell.Current.GoToAsync("..");

        // Undo uses the move history stored in the ViewModel
        private void OnUndoClicked(object sender, EventArgs e)
        {
            _viewModel.Undo();
            MoveCountLabel.Text = $"Moves: {_viewModel.MoveCount}";
            RenderGrid();
        }

        // Put everything back to the starting position
        private void OnResetClicked(object sender, EventArgs e)
        {
            _viewModel.Reset();
            MoveCountLabel.Text = "Moves: 0";
            RenderGrid();
        }
    }
}