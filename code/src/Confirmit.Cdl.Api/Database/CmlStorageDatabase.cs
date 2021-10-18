using Confirmit.Databases;
using Confirmit.DataServices.RDataAccess;
using Confirmit.Locking;
using Confirmit.NetCore.Logging;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Confirmit.Cdl.Api.Database
{
    public class CmlStorageDatabase : ICmlStorageDatabase
    {
        private readonly Logger<CmlStorageDatabase> _logger;
        private static readonly object CreateLock = new object();

        public CmlStorageDatabase(ILoggerFactory loggerFactory = null)
        {
            if (loggerFactory != null)
                _logger = new Logger<CmlStorageDatabase>(loggerFactory);
        }

        private string DatabaseName { get; } = "CmlStorage";

        private void LogException(Exception exception, string message)
        {
            if (_logger == null)
                Console.WriteLine($"ERROR: {message}");
            else
                _logger.ErrorException(exception, message);
        }

        private void LogInfo(string message)
        {
            if (_logger == null)
                Console.WriteLine(message);
            else
                _logger.Info(message);
        }

        public IConnectInfo GetConnectInfo()
        {
            return DbLib.GetConnectInfo(DatabaseName);
        }

        public SqlConnection GetConnection()
        {
            return SqlConnectionBuilder.GetSqlConnection(GetConnectInfo());
        }

        public void Upgrade()
        {
            LogInfo("Start upgrading CmlStorage database");
            try
            {
                CreateDatabaseIfNeeded();
                CreateVersionInfoTableIfNeeded();

                using var connection = GetConnection();
                var connectionString = connection.ConnectionString;

                // Initialize the services
                var serviceProvider = new ServiceCollection()
                    .AddFluentMigratorCore()
                    .ConfigureRunner(
                        builder => builder
                            .AddSqlServer2016()
                            .WithGlobalConnectionString(connectionString)
                            .WithMigrationsIn(GetType().Assembly))
                    .BuildServiceProvider();

                // Instantiate the runner
                var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

                using (new AppLock("Database", "Upgrade", AppLock.LockType.Exclusive, GetConnectInfo()))
                {
                    // Run the migrations
                    runner.MigrateUp();
                }
            }
            catch (Exception e)
            {
                LogException(e, e.Message);
                throw;
            }
        }

        private void CreateDatabaseIfNeeded()
        {
            LogInfo("Create database CmlStorage if it does not exist");
            lock (CreateLock)
            {
                var masterConnectInfo = DbLib.GetMasterConnectInfo();
                using var connection = SqlConnectionBuilder.GetSqlConnection(masterConnectInfo);
                connection.Open();
                bool exists;
                using (var command = new SqlCommand("select count(*) from sys.databases where name = @Name",
                    connection))
                {
                    command.Parameters.AddWithValue("@Name", DatabaseName);
                    exists = (int) command.ExecuteScalar() == 1;
                }

                if (exists)
                {
                    LogInfo("Database CmlStorage exists, skip creation");
                    return;
                }

                using (var command = new SqlCommand($"CREATE DATABASE [{DatabaseName}]", connection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                        LogInfo("Database CmlStorage created");
                    }
                    catch (SqlException e)
                    {
                        if (e.Number != 1801)
                            throw;
                    }
                }
            }
        }

        private void CreateVersionInfoTableIfNeeded()
        {
            using (new AppLock("Database", "Upgrade", AppLock.LockType.Exclusive, GetConnectInfo()))
            {
                var masterConnectInfo = DbLib.GetMasterConnectInfo();
                using var connection = SqlConnectionBuilder.GetSqlConnection(masterConnectInfo);
                connection.Open();

                const string query = @"
USE CmlStorage
IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @1)
BEGIN
	IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @2)
	BEGIN
		CREATE TABLE [dbo].[VersionInfo](
			[Version] [bigint] NOT NULL,
			[AppliedOn] [datetime] NULL,
			[Description] [nvarchar](1024) NULL
		) ON [PRIMARY]

		INSERT INTO VersionInfo ([Version], [AppliedOn], [Description])
        SELECT row_number() OVER (ORDER BY[Id]), [ScriptAppliedDate], [Name] FROM DatabaseUpdateHistory
		SELECT [Id], GETDATE(), [Name] FROM DatabaseUpdateHistory
	END
END";
                using var command = new SqlCommand(query, connection);
                command.Parameters.Add("@1", SqlDbType.NVarChar).Value = "DatabaseUpdateHistory";
                command.Parameters.Add("@2", SqlDbType.NVarChar).Value = "VersionInfo";

                try
                {
                    command.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    LogException(e, "Unable to execute SQL query to create table VersionInfo when it does not exist");
                    throw;
                }
            }
        }
    }
}
