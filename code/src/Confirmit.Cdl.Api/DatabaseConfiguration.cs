//==== DO NOT MODIFY THIS FILE ====
namespace Confirmit.Cdl.Api
{
    public static partial class DatabaseConfiguration
    {
        public static void ConfigureDatabase()
        {
            ConfigureDatabaseImpl();
        }

        static partial void ConfigureDatabaseImpl();

    }
}