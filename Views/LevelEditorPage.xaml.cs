using Microsoft.Maui.Controls.Shapes;
using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class LevelEditorPage : ContentPage
    {
        private readonly PersistenceService _persistence;

        // Keeps track of which tool I have selected
        private char _selectedTool = '#';

        // Stores what I have placed on the editor grid
        private char[,] _editorGrid;

        // Start with an 8x8 grid
        private int _gridRows = 8;
        private int _gridCols = 8;

        // Size of each square in the editor
        private const int CellSize = 40;

        // Used to make sure the level has been tested before saving
        private bool _hasBeenTested = false;

        public LevelEditorPage(PersistenceService persistence)
        {
            InitializeComponent();
            _persistence = persistence;

            // Create the empty grid when the editor opens
            _editorGrid = new char[_gridRows, _gridCols];
            InitEditorGrid();
            RenderEditorGrid();
        }

        private void InitEditorGrid()
        {
            // Start every cell as an empty space
            for (int r = 0; r < _gridRows; r++)
                for (int c = 0; c < _gridCols; c++)
                    _editorGrid[r, c] = ' ';
        }

        private void RenderEditorGrid()
        {
            // Remove the old grid before drawing it again
            EditorGrid.Children.Clear();
            EditorGrid.RowDefinitions.Clear();
            EditorGrid.ColumnDefinitions.Clear();

            // Create the rows and columns for the current grid size
            for (int r = 0; r < _gridRows; r++)
                EditorGrid.RowDefinitions.Add(new RowDefinition(CellSize));

            for (int c = 0; c < _gridCols; c++)
                EditorGrid.ColumnDefinitions.Add(new ColumnDefinition(CellSize));

            // Go through each cell and put it on the screen
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
            // Change the cell colour depending on what is placed there
            var border = new Border
            {
                BackgroundColor = GetCellColour(symbol),
                Padding = 0,
                Margin = 1,
                StrokeShape = new RoundRectangle { CornerRadius = 3 },
                Stroke = Colors.Gray
            };

            // Show the player as an emoji instead of the @ symbol
            if (symbol == '@')
                border.Content = new Label
                {
                    Text = "🧍",
                    FontSize = 16,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                };

            // Let the user tap the cell to place a tool
            var tap = new TapGestureRecognizer();
            int r = row, c = col;
            tap.Tapped += (s, e) => OnCellTapped(r, c);
            border.GestureRecognizers.Add(tap);

            return border;
        }

        private Color GetCellColour(char symbol) => symbol switch
        {
            // Each type of object has its own colour
            '#' => Colors.DarkSlateGray,
            '$' => Colors.SaddleBrown,
            '.' => Colors.LightBlue,
            '*' => Colors.Green,
            _ => Colors.LightGray
        };

        private void OnCellTapped(int row, int col)
        {
            // Only one player can be on the level at a time,
            // so remove the old player before placing a new one
            if (_selectedTool == '@')
            {
                for (int r = 0; r < _gridRows; r++)
                    for (int c = 0; c < _gridCols; c++)
                        if (_editorGrid[r, c] == '@')
                            _editorGrid[r, c] = ' ';
            }

            // Put the selected tool into the cell that was clicked
            _editorGrid[row, col] = _selectedTool;

            // Redraw the grid so the change can be seen
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
            string? result = await DisplayPromptAsync(
                "Grid Size",
                "Enter size (e.g. 8x8, max 12x12):",
                initialValue: $"{_gridRows}x{_gridCols}");

            if (result == null) return;

            var parts = result.Split('x');

            // Check that the size is between 5x5 and 12x12
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int rows) &&
                int.TryParse(parts[1], out int cols) &&
                rows >= 5 && rows <= 12 &&
                cols >= 5 && cols <= 12)
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
                // Let the user know what size they need to enter
                await DisplayAlertAsync(
                    "Invalid Size",
                    "Enter something like 8x8 (min 5x5, max 12x12)",
                    "OK");
            }
        }

        private async void OnTestClicked(object sender, EventArgs e)
        {
            // Check that the level has a player, box and target
            bool hasPlayer = false;
            bool hasBox = false;
            bool hasTarget = false;

            for (int r = 0; r < _gridRows; r++)
            {
                for (int c = 0; c < _gridCols; c++)
                {
                    if (_editorGrid[r, c] == '@')
                        hasPlayer = true;

                    if (_editorGrid[r, c] == '$')
                        hasBox = true;

                    if (_editorGrid[r, c] == '.')
                        hasTarget = true;
                }
            }

            // Don't let the user test a level that is missing something needed
            if (!hasPlayer || !hasBox || !hasTarget)
            {
                await DisplayAlertAsync(
                    "Invalid Level",
                    "Your level needs at least: one player (@), one box ($), one target (.)",
                    "OK");

                return;
            }

            // Turn the grid into a string so the game can use it
            var rows = new List<string>();

            for (int r = 0; r < _gridRows; r++)
            {
                var row = new string(
                    Enumerable.Range(0, _gridCols)
                        .Select(c => _editorGrid[r, c])
                        .ToArray());

                rows.Add(row);
            }

            var testLevel = new SokobanGame.Models.Level
            {
                Id = "test_level",
                Name = "Test Level",
                Grid = string.Join("\n", rows),
                IsBuiltIn = false
            };

            // Open the normal game page with the level I just created
            var navParam = new Dictionary<string, object>
            {
                { "Level", testLevel }
            };

            await Shell.Current.GoToAsync("GamePage", navParam);

            // Once the level has been tested, allow the user to save it
            _hasBeenTested = true;
            SaveBtn.IsEnabled = true;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Make sure the user has tested the level first
            if (!_hasBeenTested)
            {
                await DisplayAlertAsync(
                    "Test First",
                    "You must test the level before saving.",
                    "OK");

                return;
            }

            // Ask the user what they want to call their level
            string? name = await DisplayPromptAsync(
                "Save Level",
                "Enter a name for your level:",
                initialValue: "My Level");

            if (string.IsNullOrWhiteSpace(name))
                return;

            // Turn the grid into a string before saving it
            var rows = new List<string>();

            for (int r = 0; r < _gridRows; r++)
            {
                var row = new string(
                    Enumerable.Range(0, _gridCols)
                        .Select(c => _editorGrid[r, c])
                        .ToArray());

                rows.Add(row);
            }

            var newLevel = new SokobanGame.Models.Level
            {
                Id = $"custom_{DateTime.Now.Ticks}",
                Name = name,
                Grid = string.Join("\n", rows),
                IsBuiltIn = false
            };

            // Load the custom levels that have already been saved
            var existing = await _persistence.LoadCustomLevelsAsync()
                ?? new SokobanGame.Models.LevelCollection();

            // Add the new level to the list and save it
            existing.Levels.Add(newLevel);
            await _persistence.SaveCustomLevelsAsync(existing);

            await DisplayAlertAsync(
                "Saved!",
                $"'{name}' has been saved to your custom levels.",
                "OK");

            // Go back to the previous page
            await Shell.Current.GoToAsync("..");
        }
    }
}