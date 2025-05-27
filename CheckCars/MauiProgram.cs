using CheckCars.Data;
using CheckCars.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace CheckCars
{
    /// <summary>
    /// Provides the Maui application configuration and setup.
    /// </summary>
    public static class MauiProgram
    {
        /// <summary>
        /// Creates and configures the MauiApp instance.
        /// </summary>
        /// <returns>The configured <see cref="MauiApp"/>.</returns>
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register the SQLite DB context as a scoped service
            builder.Services.AddDbContext<ReportsDBContextSQLite>();

            // Register the main page and its view model as transient services
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainPageVM>();

            // Initialize the database and ensure it is created
            var dbContext = new ReportsDBContextSQLite();
            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

#if DEBUG
            // Add debug logging when in DEBUG configuration
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
