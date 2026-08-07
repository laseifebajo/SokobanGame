using System.Text.Json;
using SokobanGame.Models;

namespace SokobanGame.Services
{
    public class LevelService
    {
        // Heres the link to where the levels are
        private const string LevelsUrl = "https://raw.githubusercontent.com/laseifebajo/SokobanGame/main/Levels/levels.json";

        private readonly PersistenceService _persistence;

        public LevelService(PersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task<LevelCollection> GetBuiltInLevelsAsync()
        {
            // Check if levels have already been saved on the device
            var saved = await _persistence.LoadLevelsAsync();

            if (saved != null && saved.Levels.Count > 0)
                return saved;

            // If there are no saved levels, download them for the first time
            try
            {
                using var http = new HttpClient();

                // Get the JSON file from GitHub
                string json = await http.GetStringAsync(LevelsUrl);

                // Convert the JSON data into Level objects
                var levels = JsonSerializer.Deserialize<LevelCollection>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                    ?? new LevelCollection();

                // Save the levels so they can be used without downloading again
                await _persistence.SaveLevelsAsync(levels);

                return levels;
            }
            catch
            {
                // If downloading fails, use some backup levels instead
                return GetFallbackLevels();
            }
        }

        private LevelCollection GetFallbackLevels()
        {
            // Basic levels used if the internet download does not work
            return new LevelCollection
            {
                Levels = new List<Level>
                {
                    new Level
                    {
                        Id = "fallback_1",
                        Name = "Level 1",
                        Grid = "#####\n#@$.#\n#####",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "fallback_2",
                        Name = "Level 2",
                        Grid = "#######\n#. @$. #\n#######",
                        IsBuiltIn = true
                    }
                }
            };
        }
    }
}