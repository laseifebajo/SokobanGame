using System.Text.Json;
using SokobanGame.Models;

namespace SokobanGame.Services
{
    public class LevelService
    {
        // This is where the built in levels are stored online
        // The app downloads them the first time it needs them
        private const string LevelsUrl = "https://raw.githubusercontent.com/laseifebajo/SokobanGame/main/Levels/levels.json";

        private readonly PersistenceService _persistence;

        public LevelService(PersistenceService persistence)
        {
            _persistence = persistence;
        }

        public async Task<LevelCollection> GetBuiltInLevelsAsync()
        {
            // First check if the levels have already been saved on the device
            // This means we don't need to download them every time
            var saved = await _persistence.LoadLevelsAsync();

            if (saved != null && saved.Levels.Count > 0)
                return saved;

            // If there are no saved levels, try to get them from GitHub
            try
            {
                using var http = new HttpClient();

                // Stop waiting if the download takes too long
                http.Timeout = TimeSpan.FromSeconds(10);

                string json = await http.GetStringAsync(LevelsUrl);

                // Turn the JSON file into the LevelCollection used by the app
                var levels = JsonSerializer.Deserialize<LevelCollection>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                    ?? GetFallbackLevels();

                // Save the levels locally so they can be used without downloading again
                await _persistence.SaveLevelsAsync(levels);

                return levels;
            }
            catch
            {
                // If the download doesn't work, use the levels stored in the app instead
                // This means the game can still work without an internet connection
                var fallback = GetFallbackLevels();

                await _persistence.SaveLevelsAsync(fallback);

                return fallback;
            }
        }

        // These levels are here as a backup in case the GitHub file can't be downloaded
        private LevelCollection GetFallbackLevels()
        {
            return new LevelCollection
            {
                Levels = new List<Level>
                {
                    new Level
                    {
                        Id = "builtin_1",
                        Name = "Level 1 - Warm Up",
                        Grid = "######\n#    #\n#@$. #\n#    #\n######",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_2",
                        Name = "Level 2 - First Corner",
                        Grid = "#######\n#.    #\n#     #\n#  $  #\n#  @  #\n#######",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_3",
                        Name = "Level 3 - Two Boxes",
                        Grid = "#######\n#     #\n# ..  #\n# $$  #\n#  @  #\n#######",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_4",
                        Name = "Level 4 - Side by Side",
                        Grid = "########\n#      #\n# ..   #\n# $$@  #\n#      #\n########",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_5",
                        Name = "Level 5 - The Room",
                        Grid = "#########\n#       #\n# #.  # #\n# # $ # #\n# #   # #\n# #.@ # #\n#       #\n#########",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_6",
                        Name = "Level 6 - Three in a Row",
                        Grid = "#########\n#       #\n#  ...  #\n#  $$$  #\n#   @   #\n#########",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_7",
                        Name = "Level 7 - Bottleneck",
                        Grid = "##########\n#        #\n# ##  ## #\n# #.$.$# #\n# #    # #\n# #  @ # #\n# ##  ## #\n#        #\n##########",
                        IsBuiltIn = true
                    },

                    new Level
                    {
                        Id = "builtin_8",
                        Name = "Level 8 - The Maze",
                        Grid = "##########\n#   #    #\n# $ # $  #\n# . #  . #\n#   ##   #\n# @      #\n#   ##   #\n# $ #  $ #\n# . #  . #\n##########",
                        IsBuiltIn = true
                    }
                }
            };
        }
    }
}