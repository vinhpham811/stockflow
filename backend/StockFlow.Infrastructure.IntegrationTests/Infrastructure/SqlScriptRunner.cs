using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Chạy file .sql giống SSMS: tách thành từng batch theo dòng "GO".
    /// (SqlCommand không hiểu "GO" – đó là lệnh riêng của SSMS/sqlcmd.)
    /// </summary>
    public static class SqlScriptRunner
    {
        private static readonly Regex GoSeperator =
            new(@"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        public static async Task RunFileAsync(string connectionString, string filePath)
        {
            var script = await File.ReadAllTextAsync(filePath);
            var batches = GoSeperator.Split(script);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            foreach(var batch in batches)
            {
                if (string.IsNullOrWhiteSpace(batch))
                {
                    continue;
                }
                await using var command = new SqlCommand(batch, connection);
                await command.ExecuteNonQueryAsync();
            }
        }
        /// <summary>
        /// Đi ngược lên từ thư mục bin của test cho tới khi thấy thư mục "database".
        /// Chạy được cả trên máy local lẫn GitHub Actions.
        /// </summary>
        public static string FindDatabaseFolder()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while(directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "database");
                if(File.Exists(Path.Combine(candidate, "01_create_database.sql")))
                {
                    return candidate;
                }
                directory = directory.Parent;
            }
            throw new DirectoryNotFoundException("Cannot find the 'database' folder that contains the SQL scripts.");
        }
    }

    
}
