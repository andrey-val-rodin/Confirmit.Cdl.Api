using Confirmit.Cdl.Api.Accounts;
using Confirmit.Configuration;
using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Confirmit.Cdl.Api.Tools.Database
{
    /// <summary>
    /// Reads accounts from internal database
    /// </summary>
    public class QueryProvider
    {
        private readonly ConnectInfo _conn = DbLib.GetConnectInfo("CmlStorage");

        #region SELECT

        public async Task<User> GetUserAsync(int userId)
        {
            const string query = "SELECT TOP (1) Id, Name, FirstName, LastName, CompanyId FROM Users WHERE Id = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.Int).Value = userId;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
                return new User
                {
                    UserId = await reader.GetFieldValueAsync<int>(0).ConfigureAwait(false),
                    UserName = await reader.GetFieldValueAsync<string>(1).ConfigureAwait(false),
                    FirstName = await reader.IsDBNullAsync(2)
                        ? null
                        : await reader.GetFieldValueAsync<string>(2).ConfigureAwait(false),
                    LastName = await reader.IsDBNullAsync(3)
                        ? null
                        : await reader.GetFieldValueAsync<string>(3).ConfigureAwait(false),
                    CompanyId = await reader.GetFieldValueAsync<int>(4).ConfigureAwait(false)
                };

            return null;
        }

        public async Task<Company> GetCompanyAsync(int companyId)
        {
            const string query = "SELECT TOP (1) Id, Name FROM Companies WHERE Id = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.Int).Value = companyId;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
                return new Company
                {
                    CompanyId = await reader.GetFieldValueAsync<int>(0).ConfigureAwait(false),
                    Name = await reader.IsDBNullAsync(1)
                        ? null
                        : await reader.GetFieldValueAsync<string>(1).ConfigureAwait(false)
                };

            return null;
        }

        public async Task<Enduser> GetEnduserAsync(int enduserId)
        {
            const string query =
                "SELECT TOP (1) Id, Name, FirstName, LastName, ListId, CompanyId, Email FROM Endusers WHERE Id = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.Int).Value = enduserId;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
                return new Enduser
                {
                    Id = await reader.GetFieldValueAsync<int>(0).ConfigureAwait(false),
                    Name = await reader.GetFieldValueAsync<string>(1).ConfigureAwait(false),
                    FirstName = await reader.IsDBNullAsync(2)
                        ? null
                        : await reader.GetFieldValueAsync<string>(2).ConfigureAwait(false),
                    LastName = await reader.IsDBNullAsync(3)
                        ? null
                        : await reader.GetFieldValueAsync<string>(3).ConfigureAwait(false),
                    ListId = await reader.GetFieldValueAsync<int>(4).ConfigureAwait(false),
                    CompanyId = await reader.GetFieldValueAsync<int>(5).ConfigureAwait(false),
                    Email = await reader.IsDBNullAsync(6)
                        ? null
                        : await reader.GetFieldValueAsync<string>(6).ConfigureAwait(false),
                    IsActive = true
                };

            return null;
        }

        public async Task<EnduserList> GetEnduserListAsync(int listId)
        {
            const string query = "SELECT TOP (1) Id, Name FROM EnduserLists WHERE Id = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.Int).Value = listId;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
                return new EnduserList
                {
                    Id = await reader.GetFieldValueAsync<int>(0).ConfigureAwait(false),
                    Name = await reader.IsDBNullAsync(1)
                        ? null
                        : await reader.GetFieldValueAsync<string>(1).ConfigureAwait(false)
                };

            return null;
        }

        public async Task<EnduserCompany> GetEnduserCompanyAsync(int companyId)
        {
            const string query = "SELECT TOP (1) Id, Name FROM EnduserCompanies WHERE Id = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.Int).Value = companyId;

            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            if (await reader.ReadAsync().ConfigureAwait(false))
                return new EnduserCompany
                {
                    Id = await reader.GetFieldValueAsync<int>(0).ConfigureAwait(false),
                    Name = await reader.IsDBNullAsync(1)
                        ? null
                        : await reader.GetFieldValueAsync<string>(1).ConfigureAwait(false)
                };

            return null;
        }

        #endregion

        #region INSERT

        public async Task InsertUserAsync(User user)
        {
            const string query = "INSERT Users(Id, Name, FirstName, LastName, CompanyId) VALUES(@1, @2, @3, @4, @5)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.Int).Value = user.UserId;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = user.UserName;
            command.Parameters.Add("@3", SqlDbType.NVarChar).Value = user.FirstName;
            command.Parameters.Add("@4", SqlDbType.NVarChar).Value = user.LastName;
            command.Parameters.Add("@5", SqlDbType.Int).Value = user.CompanyId;

            await CatchPrimaryKeyViolationAsync(ExecuteNonQueryAsync, command).ConfigureAwait(false);
        }

        public async Task InsertCompanyAsync(Company company)
        {
            const string query = "INSERT Companies(Id, Name) VALUES(@1, @2)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.Int).Value = company.CompanyId;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = company.Name;

            await CatchPrimaryKeyViolationAsync(ExecuteNonQueryAsync, command).ConfigureAwait(false);
        }

        public async Task InsertEnduserAsync(Enduser user)
        {
            const string query =
                "INSERT Endusers(Id, Name, FirstName, LastName, ListId, CompanyId, Email) VALUES(@1, @2, @3, @4, @5, @6, @7)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.Int).Value = user.Id;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = user.Name;
            command.Parameters.Add("@3", SqlDbType.NVarChar).Value = StringOrDbNull(user.FirstName);
            command.Parameters.Add("@4", SqlDbType.NVarChar).Value = StringOrDbNull(user.LastName);
            command.Parameters.Add("@5", SqlDbType.Int).Value = user.ListId;
            command.Parameters.Add("@6", SqlDbType.Int).Value = user.CompanyId;
            command.Parameters.Add("@7", SqlDbType.NVarChar).Value = StringOrDbNull(user.Email);

            await CatchPrimaryKeyViolationAsync(ExecuteNonQueryAsync, command).ConfigureAwait(false);
        }

        public async Task InsertEnduserListAsync(EnduserList enduserList)
        {
            const string query = "INSERT EnduserLists(Id, Name) VALUES(@1, @2)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.Int).Value = enduserList.Id;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = enduserList.Name;

            await CatchPrimaryKeyViolationAsync(ExecuteNonQueryAsync, command).ConfigureAwait(false);
        }

        public async Task InsertEnduserCompanyAsync(EnduserCompany company)
        {
            const string query = "INSERT EnduserCompanies(Id, Name) VALUES(@1, @2)";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.Int).Value = company.Id;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = company.Name;

            await CatchPrimaryKeyViolationAsync(ExecuteNonQueryAsync, command).ConfigureAwait(false);
        }

        #endregion

        #region UPDATE

        public async Task UpdateUserAsync(User user)
        {
            const string query =
                "UPDATE Users SET Name = @1, FirstName = @2, LastName = @3, CompanyId = @4 WHERE Id = @5";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = user.UserName;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = user.FirstName;
            command.Parameters.Add("@3", SqlDbType.NVarChar).Value = user.LastName;
            command.Parameters.Add("@4", SqlDbType.Int).Value = user.CompanyId;
            command.Parameters.Add("@5", SqlDbType.Int).Value = user.UserId;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task UpdateCompanyAsync(Company company)
        {
            const string query = "UPDATE Companies SET Name = @1 WHERE Id = @2";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = company.Name;
            command.Parameters.Add("@2", SqlDbType.Int).Value = company.CompanyId;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task UpdateEnduserAsync(Enduser user)
        {
            const string query =
                "UPDATE Endusers SET Name = @1, FirstName = @2, LastName = @3, ListId = @4, CompanyId = @5, Email = @6 WHERE Id = @7";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = user.Name;
            command.Parameters.Add("@2", SqlDbType.NVarChar).Value = StringOrDbNull(user.FirstName);
            command.Parameters.Add("@3", SqlDbType.NVarChar).Value = StringOrDbNull(user.LastName);
            command.Parameters.Add("@4", SqlDbType.Int).Value = user.ListId;
            command.Parameters.Add("@5", SqlDbType.Int).Value = user.CompanyId;
            command.Parameters.Add("@6", SqlDbType.NVarChar).Value = StringOrDbNull(user.Email);
            command.Parameters.Add("@7", SqlDbType.Int).Value = user.Id;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task UpdateEnduserListAsync(EnduserList enduserList)
        {
            const string query = "UPDATE EnduserLists SET Name = @1 WHERE Id = @2";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = enduserList.Name;
            command.Parameters.Add("@2", SqlDbType.Int).Value = enduserList.Id;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }


        public async Task UpdateEnduserCompanyAsync(EnduserCompany company)
        {
            const string query = "UPDATE EnduserCompanies SET Name = @1 WHERE Id = @2";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.NVarChar).Value = company.Name;
            command.Parameters.Add("@2", SqlDbType.Int).Value = company.Id;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task RemoveReferencesToRevisionAsync(long revisionId)
        {
            const string query = "UPDATE Commits SET RevisionId = NULL WHERE RevisionId = @1";
            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.BigInt).Value = revisionId;

            await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        #endregion

        #region DELETE

        public async Task<int> DeleteIndividualUserPermissionsAsync(long documentId, int companyId)
        {
            const string query = @"
                DELETE UserPermissions
                WHERE DocumentId = @1 AND UserId IN
                (SELECT 
                    Permissions.UserId
                    FROM UserPermissions AS Permissions
                    INNER JOIN Users AS Users ON Permissions.UserId = Users.Id
                    WHERE Permissions.DocumentId = @1 AND Users.CompanyId = @2)
                ";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);

            await connection.OpenAsync().ConfigureAwait(false);
            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.BigInt).Value = documentId;
            command.Parameters.Add("@2", SqlDbType.Int).Value = companyId;

            return await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        public async Task<int> DeleteIndividualEnduserPermissionsAsync(long documentId, int listId)
        {
            const string query = @"
                DELETE EnduserPermissions
                WHERE DocumentId = @1 AND UserId IN
                (SELECT 
                    Permissions.UserId
                    FROM EnduserPermissions AS Permissions
                    INNER JOIN Endusers AS Endusers ON Permissions.UserId = Endusers.Id
                    WHERE Permissions.DocumentId = @1 AND Endusers.ListId = @2)
                ";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);

            command.Parameters.Add("@1", SqlDbType.BigInt).Value = documentId;
            command.Parameters.Add("@2", SqlDbType.Int).Value = listId;

            return await ExecuteNonQueryAsync(command).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs SQL query to delete all outdated documents from CML database
        /// </summary>
        /// <returns>Number of deleted documents</returns>
        public async Task<int> CleanupAsync(TimeSpan expirationPeriod)
        {
            const string query = @"
                SET DEADLOCK_PRIORITY LOW
                DELETE FROM [CmlStorage].[dbo].[Documents] WHERE Deleted <= @1";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);

            var datetime = DateTime.UtcNow - expirationPeriod;
            command.Parameters.Add("@1", SqlDbType.DateTime).Value = datetime;
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        public async Task<int> PhysicallyDeleteDocumentAsync(long documentId)
        {
            const string query = @"
                SET DEADLOCK_PRIORITY LOW
                DELETE FROM [CmlStorage].[dbo].[Documents] WHERE Id = @1";

            await using var connection = SqlConnectionBuilder.GetSqlConnection(_conn);
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@1", SqlDbType.BigInt).Value = documentId;
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        #endregion

        #region Helpers

        private static async Task<int> ExecuteNonQueryAsync(SqlCommand command)
        {
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }

        private static async Task CatchPrimaryKeyViolationAsync(Func<SqlCommand, Task> func, SqlCommand command)
        {
            try
            {
                await func(command).ConfigureAwait(false);
            }
            catch (SqlException e)
            {
                const int primaryKeyViolation = 2627;
                if (e.Number == primaryKeyViolation)
                {
                    // Attempt to insert the same entity in different threads concurrently
                    return;
                }

                throw;
            }
        }

        private static object StringOrDbNull(string value)
        {
            return string.IsNullOrEmpty(value) ? (object) DBNull.Value : value;
        }

        #endregion
    }
}