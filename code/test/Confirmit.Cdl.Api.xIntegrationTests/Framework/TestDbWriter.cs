using Confirmit.Configuration;
using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using JetBrains.Annotations;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.xIntegrationTests.Framework
{
    [PublicAPI]
    public class TestDbWriter
    {
        private readonly ConnectInfo _conn = DbLib.GetConnectInfo("CmlStorage");

        public async Task DeleteAllTestDocumentsAsync(string prefix)
        {
            const string query = "DELETE FROM Documents WHERE Name LIKE @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.NVarChar).Value = $"{prefix}%";
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task SetEnduserInactiveAsync(int enduserId)
        {
            const string query = "UPDATE enduser_users SET IsActive = 0 WHERE user_id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(DbLib.GetConfirmConnectInfo());
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = enduserId;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteUserAsync(int id)
        {
            const string query = "DELETE FROM Users WHERE Id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteCompanyAsync(int id)
        {
            await DeleteAllUsersAsync(id);

            const string query = "DELETE FROM Companies WHERE Id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private async Task DeleteAllUsersAsync(int companyId)
        {
            const string query = "DELETE FROM Users WHERE CompanyId = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = companyId;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteEnduserAsync(int id)
        {
            const string query = "DELETE FROM Endusers WHERE Id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteEnduserListAsync(int id)
        {
            await DeleteAllEndusersAsync(id);

            const string query = "DELETE FROM EnduserLists WHERE Id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task DeleteEnduserCompanyAsync(int id, int listId)
        {
            await DeleteAllEndusersAsync(listId);

            const string query = "DELETE FROM EnduserCompanies WHERE Id = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = id;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private async Task DeleteAllEndusersAsync(int enduserListId)
        {
            const string query = "DELETE FROM Endusers WHERE ListId = @p1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@p1", SqlDbType.Int).Value = enduserListId;
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
    }
}