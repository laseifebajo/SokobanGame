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
            // Show the correct image for the selected cell
            var image = new Image
            {
                Source = GetImageSource(symbol),
                Aspect = Aspect.AspectFill
            };

            var border = new Border
            {
                Content = image,
                Padding = 0,
                Margin = 0, // Removed margin to make the tiles sit flush against each other
                StrokeShape = new Rectangle(), // Changed to flat rectangle for seamless tiles
                Stroke = Colors.Transparent // Hide the border stroke
            };

            // Let the user tap the cell to place a tool
            var tap = new TapGestureRecognizer();
            int r = row, c = col;
            tap.Tapped += (s, e) => OnCellTapped(r, c);
            border.GestureRecognizers.Add(tap);

            return border;
        }

        // Map characters to their specific image resource files
        private string GetImageSource(char symbol) => symbol switch
        {
            '#' => "wall.png",
            '$' => "box.png",
            '.' => "target.png",
            '*' => "box_on_target.png",
            '@' => "player.png",
            _ => "ground.png"
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
            // Get the tool from the ImageButton that was clicked
            if (sender is ImageButton btn && btn.CommandParameter is string tool)
                _selectedTool = tool[0];
        }

        // (OnGridSizeClicked, OnTestClicked, and OnSaveClicked remain exactly the same)
        private async void OnGridSizeClicked(object sender, EventArgs e)
        {
            string? result = await DisplayPromptAsync(
                "Grid Size",
                "Enter size (e.g. 8x8, max 12x12):",
                initialValue: $"{_gridRows}x{_gridCols}");

            if (result == null) return;

            var parts = result.Split('x');

            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int rows) &&
                int.TryParse(parts[1], out int cols) &&
                rows >= 5 && rows <= 12 &&
                cols >= 5 && cols <= 12)
            {
                _gridRows = rows;
                _gridCols = cols;

                _editorGrid = new char[_gridRows, _gridCols];
                InitEditorGrid();
                RenderEditorGrid();
            }
            else
            {
                await DisplayAlertAsync(
                    "Invalid Size",
                    "Enter something like 8x8 (min 5x5, max 12x12)",
                    "OK");
            }
        }

        private async void OnTestClicked(object sender, EventArgs e)
        {
            bool hasPlayer = false;
            bool hasBox = false;
            bool hasTarget = false;

            for (int r = 0; r < _gridRows; r++)
            {
                for (int c = 0; c < _gridCols; c++)
                {
                    if (_editorGrid[r, c] == '@') hasPlayer = true;
                    if (_editorGrid[r, c] == '$') hasBox = true;
                    if (_editorGrid[r, c] == '.') hasTarget = true;
                }
            }

            if (!hasPlayer || !hasBox || !hasTarget)
            {
                await DisplayAlertAsync(
                    "Invalid Level",
                    "Your level needs at least: one player (@), one box ($), one target (.)",
                    "OK");
                return;
            }

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

            var navParam = new Dictionary<string, object>
            {
                { "Level", testLevel }
            };

            await Shell.Current.GoToAsync("GamePage", navParam);

            _hasBeenTested = true;
            SaveBtn.IsEnabled = true;
        }

        private async void OnSaveClicked(object sender, EventArgs e)
{
    if (!_hasBeenTested)
    {
        await DisplayAlert("Test First", "You must test the level before saving.", "OK");
        return;
    }

    try
    {
        string? name = await DisplayPromptAsync("Save Level", "Enter a name for your level:", initialValue: "My Level");

        if (string.IsNullOrWhiteSpace(name))
            return;

        var rows = new List<string>();
        for (int r = 0; r < _gridRows; r++)
        {
            var row = new string(Enumerable.Range(0, _gridCols).Select(c => _editorGrid[r, c]).ToArray());
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

        if (existing.Levels == null)
        {
            existing.Levels = new List<SokobanGame.Models.Level>();
        }

        existing.Levels.Add(newLevel);
        await _persistence.SaveCustomLevelsAsync(existing);

        await DisplayAlert("Saved!", $"'{name}' has been saved to your custom levels.", "OK");

        await Shell.Current.GoToAsync("..");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", ex.Message, "OK");
    }
}
    }
}