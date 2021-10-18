using Confirmit.DataServices.RDataAccess;
using JetBrains.Annotations;
using System.Data.SqlClient;

namespace Confirmit.Cdl.Api.Database
{
    [PublicAPI]
    public interface ICmlStorageDatabase
    {
        SqlConnection GetConnection();
        void Upgrade();
        IConnectInfo GetConnectInfo();
    }
}