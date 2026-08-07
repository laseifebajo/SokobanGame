using Microsoft.Extensions.Logging;
using SokobanGame.Services;
using SokobanGame.ViewModels;
using SokobanGame.Views;

namespace SokobanGame
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // These are the main services used by the game
            // Singleton means the same one is used throughout the app
            builder.Services.AddSingleton<PersistenceService>();
            builder.Services.AddSingleton<LevelService>();
            builder.Services.AddSingleton<GameEngine>();
            builder.Services.AddSingleton<SoundService>();

            // GameViewModel handles the game screen and player moves
            builder.Services.AddTransient<GameViewModel>();

            // Add all the pages so they can be used for navigation
            builder.Services.AddTransient<MainMenuPage>();
            builder.Services.AddTransient<LevelSelectPage>();
            builder.Services.AddTransient<GamePage>();
            builder.Services.AddTransient<LevelEditorPage>();
            builder.Services.AddTransient<SettingsPage>();

#if DEBUG
            // Useful for seeing debug messages while developing
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}