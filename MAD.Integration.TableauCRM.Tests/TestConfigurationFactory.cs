using Microsoft.Extensions.Configuration;

namespace MAD.Integration.TableauCRM.Tests
{
    internal class TestConfigurationFactory
    {
        public static IConfiguration Create()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", false);

            return config.Build();
        }

        public static AppConfig GetTestConfig()
        {
            var config = Create();
            var appConfig = new AppConfig();

            config.Bind(appConfig);

            return appConfig;
        }
    }
}
