using SokobanGame.Models;
using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class LevelSelectPage : ContentPage
    {
        // These services provide the built-in levels and saved player data.
        private readonly LevelService _levelService;
        private readonly PersistenceService _persistence;

        // Stores the levels in a format that can be displayed by the CollectionView.
        private List<LevelDisplayItem> _items = new();

        public LevelSelectPage(LevelService levelService, PersistenceService persistence)
        {
            InitializeComponent();

            _levelService = levelService;
            _persistence = persistence;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Reload the levels whenever this page is opened.
            await LoadLevels();
        }

        private async Task LoadLevels()
        {
            // Get the built-in levels, custom levels and the player's saved progress.
            var levelCollection = await _levelService.GetBuiltInLevelsAsync();
            var customCollection = await _persistence.LoadCustomLevelsAsync();
            var progress = await _persistence.LoadProgressAsync() ?? new List<LevelProgress>();

            _items = new List<LevelDisplayItem>();

            // Add all of the built-in levels to the list shown to the player.
            foreach (var level in levelCollection.Levels)
            {
                // Find any saved progress for this particular level.
                var prog = progress.FirstOrDefault(p => p.LevelId == level.Id);

                _items.Add(new LevelDisplayItem
                {
                    Level = level,
                    Name = level.Name,
                    IsCompleted = prog?.IsCompleted ?? false,
                    BestMovesText = prog?.IsCompleted == true ? $"Best: {prog.BestMoves}" : ""
                });
            }

            // Custom levels are added after the built-in levels.
            if (customCollection != null)
            {
                foreach (var level in customCollection.Levels)
                {
                    // Check if the player has already completed this custom level.
                    var prog = progress.FirstOrDefault(p => p.LevelId == level.Id);

                    _items.Add(new LevelDisplayItem
                    {
                        Level = level,
                        Name = level.Name,
                        IsCompleted = prog?.IsCompleted ?? false,
                        BestMovesText = prog?.IsCompleted == true ? $"Best: {prog.BestMoves}" : ""
                    });
                }
            }

            // Give the finished list to the CollectionView so it can display the levels.
            LevelsCollection.ItemsSource = _items;
        }

        private async void OnLevelSelected(object sender, SelectionChangedEventArgs e)
        {
            // Make sure the selected item is actually a level before continuing.
            if (e.CurrentSelection.FirstOrDefault() is not LevelDisplayItem item) return;

            // Clear the selection so the same level can be selected again later.
            LevelsCollection.SelectedItem = null;

            // Pass the selected level to the game page.
            var navParam = new Dictionary<string, object>
            {
                { "Level", item.Level }
            };

            await Shell.Current.GoToAsync("GamePage", navParam);
        }
    }

    // Holds the information needed to display a level in the level selection screen.
    public class LevelDisplayItem
    {
        public Level Level { get; set; } = new();
        public string Name { get; set; } = "";
        public bool IsCompleted { get; set; }
        public string BestMovesText { get; set; } = "";
    }
}