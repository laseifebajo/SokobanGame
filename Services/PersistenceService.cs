using System.Text.Json;
using SokobanGame.Models;

namespace SokobanGame.Services
{
    public class PersistenceService
    {
        // This is where the app stores saved files on the device
        private readonly string _dataPath = FileSystem.AppDataDirectory;

        // Saves any type of data as a JSON file
        public async Task SaveAsync<T>(string filename, T data)
        {
            string path = Path.Combine(_dataPath, filename);

            // Convert the data into JSON format
            string json = JsonSerializer.Serialize(data);

            // Write the JSON file to storage
            await File.WriteAllTextAsync(path, json);
        }

        // Loads a JSON file and returns the data
        // Returns null if the file does not exist
        public async Task<T?> LoadAsync<T>(string filename)
        {
            string path = Path.Combine(_dataPath, filename);

            if (!File.Exists(path))
                return default;

            // Read the saved JSON file
            string json = await File.ReadAllTextAsync(path);

            // Convert JSON back into the object
            return JsonSerializer.Deserialize<T>(json);
        }

        // Save the built-in levels
        public async Task SaveLevelsAsync(LevelCollection levels)
            => await SaveAsync("levels.json", levels);

        // Load the built-in levels
        public async Task<LevelCollection?> LoadLevelsAsync()
            => await LoadAsync<LevelCollection>("levels.json");

        // Save player progress
        public async Task SaveProgressAsync(List<LevelProgress> progress)
            => await SaveAsync("progress.json", progress);

        // Load player progress
        public async Task<List<LevelProgress>?> LoadProgressAsync()
            => await LoadAsync<List<LevelProgress>>("progress.json");

        // Save custom levels created by the user
        public async Task SaveCustomLevelsAsync(LevelCollection levels)
            => await SaveAsync("custom_levels.json", levels);

        // Load custom levels
        public async Task<LevelCollection?> LoadCustomLevelsAsync()
            => await LoadAsync<LevelCollection>("custom_levels.json");

        // Save user settings
        public async Task SaveSettingsAsync(AppSettings settings)
            => await SaveAsync("settings.json", settings);

        // Load user settings, or create default settings if none exist
        public async Task<AppSettings> LoadSettingsAsync()
            => await LoadAsync<AppSettings>("settings.json") ?? new AppSettings();
    }
}