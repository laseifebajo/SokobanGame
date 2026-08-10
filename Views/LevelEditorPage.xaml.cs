using Microsoft.Maui.Controls.Shapes;
using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class LevelEditorPage : ContentPage
    {
        private readonly PersistenceService _persistence;

        // This keeps track of what tool is currently selected
        private char _selectedTool = '#';

        // This is the grid where I build the level
        private char[,] _editorGrid;

        // Start the editor with an 8x8 grid
        private int _gridRows = 8;
        private int _gridCols = 8;

        // Size of each square in the editor
        private const int CellSize = 40;

        private bool _hasBeenTested = false;

        public LevelEditorPage(PersistenceService persistence)
        {
            InitializeComponent();
            _persistence = persistence;

            // Make the empty grid when the page opens
            _editorGrid = new char[_gridRows, _gridCols];
            InitEditorGrid();
            RenderEditorGrid();
        }

        private void InitEditorGrid()
        {
            // Fill the grid with empty spaces to start with
            for (int r = 0; r < _gridRows; r++)
                for (int c = 0; c < _gridCols; c++)
                    _editorGrid[r, c] = ' ';
        }

        private void RenderEditorGrid()
        {
            // Clear the old grid before drawing the new one
            EditorGrid.Children.Clear();
            EditorGrid.RowDefinitions.Clear();
            EditorGrid.ColumnDefinitions.Clear();

            // Add the rows and columns needed for the current grid size
            for (int r = 0; r < _gridRows; r++)
                EditorGrid.RowDefinitions.Add(new RowDefinition(CellSize));

            for (int c = 0; c < _gridCols; c++)
                EditorGrid.ColumnDefinitions.Add(new ColumnDefinition(CellSize));

            // Go through the grid and create each square on screen
            for (int r = 0; r < _gridRows; r++)
            {
                for (int c = 0; c < _gridCols; c++)
                {
                    var cell = CreateEditorCell(_editorGrid[r, c], r, c);

                    Grid.SetRow(cell, r);
                    Grid.SetColumn(cell, c);

                    EditorGrid.Children.Add(cell);
                }
            }
        }

        private View CreateEditorCell(char symbol, int row, int col)
        {
            // The colour of the square depends on what is inside it
            var border = new Border
            {
                BackgroundColor = GetCellColour(symbol),
                Padding = 0,
                Margin = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 3 },
                Stroke = Colors.Gray
            };

            // Show the player as an emoji instead of just showing the @ symbol
            if (symbol == '@')
                border.Content = new Label
                {
                    Text = "🧍",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

            // Let the user tap a square to place the selected tool
            var tap = new TapGestureRecognizer();
            int r = row, c = col;
            tap.Tapped += (s, e) => OnCellTapped(r, c);
            border.GestureRecognizers.Add(tap);

            return border;
        }

        private Color GetCellColour(char symbol) => symbol switch
        {
            // Give each type of object its own colour
            '#' => Colors.DarkSlateGray,
            '$' => Colors.SaddleBrown,
            '.' => Colors.LightBlue,
            '*' => Colors.Green,
            _   => Colors.LightGray
        };

        private void OnCellTapped(int row, int col)
        {
            // There can only be one player, so remove the old one
            // if the user decides to place the player somewhere else
            if (_selectedTool == '@')
            {
                for (int r = 0; r < _gridRows; r++)
                    for (int c = 0; c < _gridCols; c++)
                        if (_editorGrid[r, c] == '@')
                            _editorGrid[r, c] = ' ';
            }

            // Put whatever tool was selected into the square.
            _editorGrid[row, col] = _selectedTool;

            // Draw the grid again so the change shows up.
            RenderEditorGrid();
        }

        private void ToolSelected(object sender, EventArgs e)
        {
            // Get the tool from the button that was clicked
            if (sender is Button btn && btn.CommandParameter is string tool)
                _selectedTool = tool[0];
        }

        private async void OnGridSizeClicked(object sender, EventArgs e)
        {
            // Ask the user what size they want the level to be
            string? result = await DisplayPromptAsync("Grid Size",
                "Enter size (e.g. 8x8, max 12x12):",
                initialValue: $"{_gridRows}x{_gridCols}");

            if (result == null) return;

            var parts = result.Split('x');

            // Check that the size entered is between 5x5 and 12x12
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int rows) &&
                int.TryParse(parts[1], out int cols) &&
                rows >= 5 && rows <= 12 && cols >= 5 && cols <= 12)
            {
                _gridRows = rows;
                _gridCols = cols;

                // Make a new empty grid with the new size
                _editorGrid = new char[_gridRows, _gridCols];
                InitEditorGrid();
                RenderEditorGrid();
            }
            else
            {
                // Tell the user if they entered an invalid size
                await DisplayAlertAsync("Invalid Size",
                    "Enter something like 8x8 (min 5x5, max 12x12)", "OK");
            }
        }

        // These features are going to be added later
        private async void OnTestClicked(object sender, EventArgs e)
        {
            // Validate level has minimum required elements
            bool hasPlayer = false, hasBox = false, hasTarget = false;
            for (int r = 0; r < _gridRows; r++)
                for (int c = 0; c < _gridCols; c++)
                {
                    if (_editorGrid[r, c] == '@') hasPlayer = true;
                    if (_editorGrid[r, c] == '$') hasBox = true;
                    if (_editorGrid[r, c] == '.') hasTarget = true;
                }

            if (!hasPlayer || !hasBox || !hasTarget)
            {
                await DisplayAlertAsync("Invalid Level",
                    "Your level needs at least: one player (@), one box ($), one target (.)", "OK");
                return;
            }

            // Build the grid string
            var rows = new List<string>();
            for (int r = 0; r < _gridRows; r++)
            {
                var row = new string(Enumerable.Range(0, _gridCols)
                    .Select(c => _editorGrid[r, c]).ToArray());
                rows.Add(row);
            }

            var testLevel = new SokobanGame.Models.Level
            {
                Id = "test_level",
                Name = "Test Level",
                Grid = string.Join("\n", rows),
                IsBuiltIn = false
            };

            // Navigate to game page with test level
            var navParam = new Dictionary<string, object> { { "Level", testLevel } };
            await Shell.Current.GoToAsync("GamePage", navParam);

            // When they return, unlock save
            _hasBeenTested = true;
            SaveBtn.IsEnabled = true;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!_hasBeenTested)
            {
                await DisplayAlertAsync("Test First",
                    "You must test the level before saving.", "OK");
                return;
            }

            string? name = await DisplayPromptAsync("Save Level",
                "Enter a name for your level:", initialValue: "My Level");
            if (string.IsNullOrWhiteSpace(name)) return;

            var rows = new List<string>();
            for (int r = 0; r < _gridRows; r++)
            {
                var row = new string(Enumerable.Range(0, _gridCols)
                    .Select(c => _editorGrid[r, c]).ToArray());
                rows.Add(row);
            }

            var newLevel = new SokobanGame.Models.Level
            {
                Id = $"custom_{DateTime.Now.Ticks}",
                Name = name,
                Grid = string.Join("\n", rows),
                IsBuiltIn = false
            };

            var existing = await _persistence.LoadCustomLevelsAsync()
                ?? new SokobanGame.Models.LevelCollection();
            existing.Levels.Add(newLevel);
            await _persistence.SaveCustomLevelsAsync(existing);

            await DisplayAlertAsync("Saved!",
                $"'{name}' has been saved to your custom levels.", "OK");

            await Shell.Current.GoToAsync("..");
        }
            }
}