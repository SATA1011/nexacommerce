using DbUp;

namespace NexaCommerce.Data.Migrations;

public static class DatabaseMigrator
{
    public static void Migrate(string connectionString, string databaseDirectoryPath)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString));
        }

        if (string.IsNullOrWhiteSpace(databaseDirectoryPath) || !Directory.Exists(databaseDirectoryPath))
        {
            return;
        }

        EnsureDatabase.For.MySqlDatabase(connectionString);

        var tableSqlPath = Path.Combine(databaseDirectoryPath, "Table.sql");
        var spSqlPath = Path.Combine(databaseDirectoryPath, "AllStoredProcedure.sql");

        if (File.Exists(tableSqlPath))
        {
            var tableScript = File.ReadAllText(tableSqlPath);
            var tableEngine = DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScript("Table.sql", tableScript)
                .LogToConsole()
                .Build();

            var result = tableEngine.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException($"Database schema migration (Table.sql) failed: {result.Error}", result.Error);
            }
        }

        if (File.Exists(spSqlPath))
        {
            var spScript = File.ReadAllText(spSqlPath);
            var spEngine = DeployChanges.To
                .MySqlDatabase(connectionString)
                .WithScript("AllStoredProcedure.sql", spScript)
                .LogToConsole()
                .Build();

            var result = spEngine.PerformUpgrade();
            if (!result.Successful)
            {
                throw new InvalidOperationException($"Database stored procedure migration (AllStoredProcedure.sql) failed: {result.Error}", result.Error);
            }
        }
    }
}
