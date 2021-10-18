using Confirmit.Cdl.Api.xIntegrationTests.Fixtures;
using Confirmit.Cdl.Api.xIntegrationTests.Framework;
using Confirmit.DataServices.RDataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Confirmit.Cdl.Api.xIntegrationTests
{
    [Collection(nameof(SharedFixture))]
    public class DbTriggerTests : TestBase, IClassFixture<DbTriggerFixture>
    {
        private readonly DbTriggerFixture _fixture;

        public DbTriggerTests(SharedFixture sharedFixture, DbTriggerFixture fixture, ITestOutputHelper outputHelper)
            : base(sharedFixture, outputHelper)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task VerifyTriggers()
        {
            await InsertDocumentAsync();
            await VerifyInsertTrigger();
            await DeleteDocumentAsync();
            await VerifyDeletionAsync();
        }

        #region Helpers

        private async Task InsertDocumentAsync()
        {
            const string query =
                "INSERT INTO Documents" +
                "(Name, SourceCode, Created, CreatedBy, CreatedByName, Modified, ModifiedBy, ModifiedByName, CompanyId, CompanyName) " +
                "VALUES(@p1, @p2, @p3, @p4, @p5, @p3, @p4, @p5, @p6, @p7)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_fixture.Conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.NVarChar).Value = "Name";
            command.Parameters.Add("@p2", SqlDbType.NVarChar).Value = "";
            command.Parameters.Add("@p3", SqlDbType.DateTime).Value = DateTime.UtcNow;
            command.Parameters.Add("@p4", SqlDbType.Int).Value = DbTriggerFixture.UserId;
            command.Parameters.Add("@p5", SqlDbType.NVarChar).Value = "Test User";
            command.Parameters.Add("@p6", SqlDbType.Int).Value = 1;
            command.Parameters.Add("@p7", SqlDbType.NVarChar).Value = "Confirmit";
            await command.ExecuteNonQueryAsync();
        }

        private async Task VerifyInsertTrigger()
        {
            const string query = "SELECT * FROM UserPermissions WHERE UserId = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_fixture.Conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = DbTriggerFixture.UserId;
            Assert.True(await command.ExecuteScalarAsync() != null, "Trigger for INSERT does not work");
        }

        private async Task DeleteDocumentAsync()
        {
            const string query = "DELETE FROM Documents WHERE CreatedBy = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_fixture.Conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = DbTriggerFixture.UserId;
            await command.ExecuteNonQueryAsync();
        }

        private async Task VerifyDeletionAsync()
        {
            const string query = "SELECT * FROM UserPermissions WHERE UserId = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_fixture.Conn);
            await connection.OpenAsync();

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = DbTriggerFixture.UserId;
            Assert.True(await command.ExecuteScalarAsync() == null,
                "Deletion of document does not delete user permissions");
        }

        #endregion
    }
}
