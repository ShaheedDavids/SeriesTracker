using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SeriesTracker.Services;
using System.Reflection;

namespace SeriesTracker
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
                });

            // Load appsettings.json from embedded resources
            /*var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("SeriesTracker.appsettings.json");

            if (stream != null)
            {
                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream)
                    .Build();

                builder.Configuration.AddConfiguration(config);
            }*/
            // Replace with this:
            var inMemoryConfig = new Dictionary<string, string?>
            {
                ["MongoDB:ConnectionString"] = "mongodb+srv://admin:admin123@cluster0.kigp57t.mongodb.net/",
                ["MongoDB:DatabaseName"] = "SeriesTracker"
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemoryConfig)
                .Build();

            builder.Configuration.AddConfiguration(config);

            // Register services as singletons so state is shared across all pages
            builder.Services.AddSingleton<MongoDbService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<WatchListService>();

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
