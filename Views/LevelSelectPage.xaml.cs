using SokobanGame.Models;
using SokobanGame.Services;

namespace SokobanGame.Views
{
    public partial class LevelSelectPage : ContentPage
    {
        // These services provide the built-in levels and saved player data
        private readonly LevelService _levelService;
        private readonly PersistenceService _persistence;

        // Stores the levels in a format that can be displayed by the CollectionView
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

            // Reload the levels whenever this page is opened
            await LoadLevels();
        }

        private async Task LoadLevels()
        {
            var levelCollection = await _levelService.GetBuiltInLevelsAsync();
            var customCollection = await _persistence.LoadCustomLevelsAsync();
            var progress = await _persistence.LoadProgressAsync() 
                ?? new List<LevelProgress>();

            _items = new List<LevelDisplayItem>();

            // Add builtin levls
            foreach (var level in levelCollection.Levels)
            {
                var prog = progress.FirstOrDefault(p => p.LevelId == level.Id);
                _items.Add(new LevelDisplayItem
                {
                    Level = level,
                    Name = level.Name,
                    IsCompleted = prog?.IsCompleted ?? false,
                    BestMovesText = prog?.IsCompleted == true
                        ? $"Best: {prog.BestMoves}" : ""
                });
            }

            // Added custom levels with a label so the user knows they made them
            if (customCollection != null)
            {
                foreach (var level in customCollection.Levels)
                {
                    var prog = progress.FirstOrDefault(p => p.LevelId == level.Id);
                    _items.Add(new LevelDisplayItem
                    {
                        Level = level,
                        Name = $"⭐ {level.Name}",
                        IsCompleted = prog?.IsCompleted ?? false,
                        BestMovesText = prog?.IsCompleted == true
                            ? $"Best: {prog.BestMoves}" : ""
                    });
                }
            }

            LevelsCollection.ItemsSource = _items;
        }

        private async void OnLevelSelected(object sender, SelectionChangedEventArgs e)
        {
            // Make sure the selected item is actually a level before continuing
            if (e.CurrentSelection.FirstOrDefault() is not LevelDisplayItem item) return;

            // Clear the selection so the same level can be selected again later
            LevelsCollection.SelectedItem = null;

            // Pass the selected level to the game page.
            var navParam = new Dictionary<string, object>
            {
                { "Level", item.Level }
            };

            await Shell.Current.GoToAsync("GamePage", navParam);
        }
    }

    // Holds the information needed to display a level in the level selection screen
    public class LevelDisplayItem
    {
        public Level Level { get; set; } = new();
        public string Name { get; set; } = "";
        public bool IsCompleted { get; set; }
        public string BestMovesText { get; set; } = "";
    }
}