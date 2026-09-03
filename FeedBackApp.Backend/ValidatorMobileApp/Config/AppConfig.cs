using Microsoft.Extensions.Configuration;

namespace ValidatorMobileApp.Config
{
    public static class AppConfig
    {
        public static readonly string BaseUrl;

        public static readonly string ApiKey;

        static AppConfig()
        {
            using var stream = FileSystem.OpenAppPackageFileAsync($"appsettings.{EnvironmentHelper.EnvironmentName}.json")
                .GetAwaiter()
                .GetResult();

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            BaseUrl = config["BaseUrl"];
            ApiKey = config["ApiKey"];
        }
    }
}
