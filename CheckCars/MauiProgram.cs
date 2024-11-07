using CheckCars.Data;
using CheckCars.Utilities;
using CheckCars.ViewModels;
using Microsoft.Extensions.Logging;
using PdfSharp.Fonts;

namespace CheckCars
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
                    fonts.AddFont("Arial.ttf", "ArialRegular");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                }).Services.AddSingleton<FontResolver>();
            builder.Services.AddDbContext<ReportsDBContextSQLite>();
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<MainPageVM>();

       var fontResolver = builder.Services.BuildServiceProvider().GetRequiredService<FontResolver>();
           GlobalFontSettings.FontResolver = fontResolver; // Registrar globalmente el font resolver


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
