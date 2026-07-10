using Microsoft.Extensions.Configuration;

namespace ValidatorMobileApp
{
    public static class AppConfig
    {
        public static readonly string BaseUrl;

        static AppConfig()
        {
            using var stream = FileSystem.OpenAppPackageFileAsync($"appsettings.dev.json")
                .GetAwaiter()
                .GetResult();

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            BaseUrl = config["BaseUrl"];
        }
    }
}
