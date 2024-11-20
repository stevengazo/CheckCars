using CheckCars.Data;
using CheckCars.Utilities;
using CheckCars.ViewModels;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;


namespace CheckCars
{
    public static class MauiProgram
    {
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
            builder.Services.AddDbContext<ReportsDBContextSQLite>();
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<MainPageVM>();

            GlobalFontSettings.FontResolver = new FileFontProvider();

            var dbContext = new ReportsDBContextSQLite();

            dbContext.Database.EnsureCreated();
            dbContext.Dispose();

            #if DEBUG
            builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
