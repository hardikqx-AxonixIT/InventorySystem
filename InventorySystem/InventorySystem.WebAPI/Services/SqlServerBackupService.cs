using System.Text.Json;
using InventorySystem.Application.Services;
using InventorySystem.WebAPI.Options;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace InventorySystem.WebAPI.Services
{
    public interface ISystemBackupService
    {
        Task<object> CreateBackupAsync(string? label, CancellationToken ct);
        Task<object> RestoreBackupAsync(string fileName, CancellationToken ct);
        Task<object> ListBackupsAsync(CancellationToken ct);
    }

    public class SqlServerBackupService : ISystemBackupService
    {
        private readonly IConfiguration _configuration;
        private readonly BackupOptions _options;
        private readonly ITransactionService _transactionService;
        public SqlServerBackupService(
            IConfiguration configuration,
            IOptions<BackupOptions> options,
            ITransactionService transactionService)
        {
            _configuration = configuration;
            _options = options.Value;
            _transactionService = transactionService;
        }

        public async Task<object> CreateBackupAsync(string? label, CancellationToken ct)
        {
            var builder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
            var dbName = builder.InitialCatalog;
            var backupDir = ResolveBackupDirectory();
            Directory.CreateDirectory(backupDir);

            var safeLabel = string.IsNullOrWhiteSpace(label) ? "manual" : new string(label.Where(char.IsLetterOrDigit).ToArray());
            var fileName = $"{dbName}_{safeLabel}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak";
            var filePath = Path.Combine(backupDir, fileName);

            try
            {
                await using var connection = new SqlConnection(builder.ConnectionString);
                await connection.OpenAsync(ct);
                await using var command = connection.CreateCommand();
                command.CommandText = $"BACKUP DATABASE [{dbName}] TO DISK = @path WITH INIT, COMPRESSION";
                command.Parameters.AddWithValue("@path", filePath);
                await command.ExecuteNonQueryAsync(ct);

                return new
                {
                    success = true,
                    mode = "sql-bak",
                    fileName,
                    filePath,
                    createdAtUtc = DateTime.UtcNow
                };
            }
            catch
            {
                var jsonFileName = $"{dbName}_{safeLabel}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
                var jsonPath = Path.Combine(backupDir, jsonFileName);
                var bootstrap = await _transactionService.GetBootstrapAsync(ct);
                var snapshot = new
                {
                    createdAtUtc = DateTime.UtcNow,
                    note = "Logical snapshot backup generated because SQL .bak operation was unavailable.",
                    bootstrap
                };

                await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(snapshot), ct);
                return new
                {
                    success = true,
                    mode = "json-snapshot",
                    fileName = jsonFileName,
                    filePath = jsonPath,
                    createdAtUtc = DateTime.UtcNow
                };
            }
        }

        public async Task<object> RestoreBackupAsync(string fileName, CancellationToken ct)
        {
            var backupDir = ResolveBackupDirectory();
            var fullPath = Path.Combine(backupDir, fileName);
            if (!File.Exists(fullPath))
            {
                throw new InvalidOperationException("Backup file not found.");
            }

            if (Path.GetExtension(fileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                return new
                {
                    success = false,
                    mode = "json-snapshot",
                    note = "JSON snapshot restore is not automatic yet. Use SQL .bak restore for full DB recovery."
                };
            }

            var sourceBuilder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("DefaultConnection"));
            var dbName = sourceBuilder.InitialCatalog;
            var masterBuilder = new SqlConnectionStringBuilder(sourceBuilder.ConnectionString)
            {
                InitialCatalog = "master"
            };

            await using var connection = new SqlConnection(masterBuilder.ConnectionString);
            await connection.OpenAsync(ct);
            await using var cmd = connection.CreateCommand();
            cmd.CommandText = $@"
ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [{dbName}] FROM DISK = @path WITH REPLACE;
ALTER DATABASE [{dbName}] SET MULTI_USER;";
            cmd.Parameters.AddWithValue("@path", fullPath);
            await cmd.ExecuteNonQueryAsync(ct);

            return new
            {
                success = true,
                restoredFrom = fileName,
                restoredAtUtc = DateTime.UtcNow
            };
        }

        public Task<object> ListBackupsAsync(CancellationToken ct)
        {
            var backupDir = ResolveBackupDirectory();
            Directory.CreateDirectory(backupDir);
            var files = new DirectoryInfo(backupDir)
                .GetFiles()
                .Where(x => x.Extension.Equals(".bak", StringComparison.OrdinalIgnoreCase) || x.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.CreationTimeUtc)
                .Select(x => new
                {
                    x.Name,
                    x.Length,
                    createdAtUtc = x.CreationTimeUtc
                })
                .ToList();

            return Task.FromResult<object>(new
            {
                directory = backupDir,
                count = files.Count,
                files
            });
        }

        private string ResolveBackupDirectory()
        {
            if (Path.IsPathRooted(_options.BackupDirectory)) return _options.BackupDirectory;
            return Path.Combine(AppContext.BaseDirectory, _options.BackupDirectory);
        }
    }
}
