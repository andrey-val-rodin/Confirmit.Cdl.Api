using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.xIntegrationTests.Clients;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.Configuration;
using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using Confirmit.NetCore.Client;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Company = Confirmit.Cdl.Api.Accounts.Company;
using User = Confirmit.Cdl.Api.Accounts.User;

namespace Confirmit.Cdl.Api.xIntegrationTests.Fixtures
{
    [UsedImplicitly]
    public class DbTriggerFixture : BaseFixture
    {
        private const int CompanyId = -1;
        public const int UserId = -1;
        public readonly ConnectInfo Conn = DbLib.GetConnectInfo("CmlStorage");

        protected override void AddLocalServices(IServiceCollection services)
        {
            var uri = GetServiceUri();
            services.AddConfirmitClient<ICdl>(uri);
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var company = new Company
            {
                CompanyId = CompanyId,
                Name = "DbTriggerTests"
            };
            var user = new User
            {
                UserId = UserId,
                CompanyId = CompanyId,
                UserName = "DbTriggerTests",
                FirstName = "first",
                LastName = "last"
            };

            var writer = new QueryProvider();
            await writer.InsertCompanyAsync(company);
            await writer.InsertUserAsync(user);
        }

        public override async Task DisposeAsync()
        {
            await DeleteDocumentAsync();
            var writer = new TestDbWriter();
            await writer.DeleteUserAsync(UserId);
            await writer.DeleteCompanyAsync(CompanyId);

            await base.DisposeAsync();
        }

        private async Task DeleteDocumentAsync()
        {
            const string query = "DELETE FROM Documents WHERE CreatedBy = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(Conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = UserId;
            await command.ExecuteNonQueryAsync();
        }
    }
}
