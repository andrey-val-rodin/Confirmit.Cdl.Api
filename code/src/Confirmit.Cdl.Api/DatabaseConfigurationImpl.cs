
using Confirmit.Cdl.Api.Database;

namespace Confirmit.Cdl.Api
{
    public partial class DatabaseConfiguration
    {
        static partial void ConfigureDatabaseImpl()
        {
            new CmlStorageDatabase().Upgrade();
        }
    }
}
