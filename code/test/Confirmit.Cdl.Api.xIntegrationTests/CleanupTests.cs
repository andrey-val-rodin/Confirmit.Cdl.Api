using Confirmit.Cdl.Api.Tools.Database;
using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.Configuration;
using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class CleanupTests : TestBase
    {
        public CleanupTests(SharedFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task CleanupQuery_OneOutdatedDocument_OutdatedDocumentDeleted()
        {
            const int expirationPeriod = 30;
            var created = DateTime.UtcNow - TimeSpan.FromDays(100);
            var deleted = DateTime.UtcNow - TimeSpan.FromDays(expirationPeriod);
            var liveDocument = await InsertAsync(created);
            var archivedDocument = await InsertAsync(created, deleted);

            await new QueryProvider().CleanupAsync(TimeSpan.FromDays(30));

            // Outdated document was deleted
            Assert.False(await ExistsAsync(archivedDocument));
            // Live document still exists
            Assert.True(await ExistsAsync(liveDocument));
        }

        [Fact]
        public async Task CleanupQuery_ZeroExpirationPeriod_SuccessfullyDeleted()
        {
            // Insert document that is deleted right now
            var archivedDocument = await InsertAsync(DateTime.UtcNow, DateTime.UtcNow);

            var deletedDocumentCount = await new QueryProvider().CleanupAsync(TimeSpan.FromDays(0));

            // Outdated document was deleted
            Assert.False(await ExistsAsync(archivedDocument));
            Assert.True(deletedDocumentCount > 0);
        }

        [Fact]
        public async Task Cleanup_OneOutdatedDocument_SuccessfullyDeleted()
        {
            // Insert document that is deleted right now
            var archivedDocument = await InsertAsync(DateTime.UtcNow, DateTime.UtcNow);
            Assert.True(await ExistsAsync(archivedDocument));

            var log = new LoggerStub<Cleanup>();
            var cleanup = new Cleanup(TimeSpan.FromMilliseconds(1), TimeSpan.FromDays(0), log);
            var token = CancellationToken.None;
            try
            {
                cleanup.Start(token);
                for (int i = 0; i < 1000; i++)
                {
                    if (log.LogEntries.Any(t => t.Item2.Contains("Database cleanup: success")))
                        break;

                    // If something goes wrong, common waiting time will be 1000 * 100 = 100000 milliseconds = 100 seconds
                    Thread.Sleep(100);
                }
            }
            finally
            {
                cleanup.Stop();
            }

            Assert.False(await ExistsAsync(archivedDocument));
        }

        #region Helpers

        private readonly ConnectInfo _conn = DbLib.GetConnectInfo("CmlStorage");

        private async Task<long> InsertAsync(DateTime created, DateTime? deleted = null)
        {
            const string name = Prefix + "Cleanup";
            const string query = @"
                INSERT INTO [CmlStorage].[dbo].[Documents] (
                    Name, Type, Created, CreatedBy, CreatedByName,
                    Modified, ModifiedBy, ModifiedByName, CompanyId, CompanyName, SourceCode, Deleted)
                VALUES (@1, 1, @2, 1, 'TestUser', @2, 1, 'TestUser', 1, 'Confirmit', '', @3)
                SELECT [Id] FROM [CmlStorage].[dbo].[Documents] WHERE [Id] = scope_identity()";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = name;
            command.Parameters.Add("@2", SqlDbType.DateTime).Value = created;
            command.Parameters.Add("@3", SqlDbType.DateTime).Value = deleted ?? (object) DBNull.Value;

            return (long) await command.ExecuteScalarAsync();
        }

        private async Task<bool> ExistsAsync(long id)
        {
            const string query = @"
                SELECT [Id] FROM [CmlStorage].[dbo].[Documents] WHERE [Id] = @1";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.BigInt).Value = id;
            var result = await command.ExecuteScalarAsync();
            return result != null && id == (long) result;
        }

        #endregion
    }
}
