namespace ValidatorMobileApp.Config
{
    public class EnvironmentHelper
    {
        private const string ProductionEnvironmentName = "prod";

        private const string DevelopmentEnvironmentName = "dev";

        public static string EnvironmentName
        {
            get
            {
#if APPENV_development
                return DevelopmentEnvironmentName;
#elif APPENV_production
                return ProductionEnvironmentName;
#endif
            }
        }
    }
}
