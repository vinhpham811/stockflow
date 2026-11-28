using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Testcontainers.MsSql;

namespace StockFlow.Infrastructure.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Chạy 1 lần cho toàn bộ test trong collection "Database":
    /// bật SQL Server bằng Docker, tạo bảng + SP bằng script thật, chạy EF Migration.
    /// </summary>
    public class DatabaseFixture : IAsyncLifetime
    {
        private const string DatabaseName = "StockFlowDB";
        private readonly MsSqlContainer _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
        public string ConnectionString { get; private set; } = string.Empty;
        public async Task InitializeAsync() {
            await _container.StartAsync();

            // Connection string mặc định của container trỏ tới database "master"
            var masterConnectionString = _container.GetConnectionString();

            // 1. Tạo database, bảng nghiệp vụ, stored procedure bằng CHÍNH script của dự án
            var databaseFolder = SqlScriptRunner.FindDatabaseFolder();
            await SqlScriptRunner.RunFileAsync(masterConnectionString, Path.Combine(databaseFolder, "01_create_database.sql"));
            await SqlScriptRunner.RunFileAsync(masterConnectionString, Path.Combine(databaseFolder, "02_create_tables.sql"));
            await SqlScriptRunner.RunFileAsync(masterConnectionString, Path.Combine(databaseFolder, "03_create_procedures.sql"));

            // 2. Đổi sang database StockFlowDb
            ConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = DatabaseName
            }.ConnectionString;

            // 3. Tạo bảng Identity + RefreshTokens bằng EF Migration
            await using var context = CreateDBContext();
            await context.Database.MigrateAsync();
        }
        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }
        /// <summary>Mỗi lần gọi trả về 1 DbContext MỚI.</summary>
        public AppDbContext CreateDBContext()
        {
            var option = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            return new AppDbContext(option);
        }

        /// <summary>Xóa sạch dữ liệu (giữ nguyên cấu trúc bảng). Xóa bảng con trước, bảng cha sau.</summary>
        public async Task ResetDatabaseAsync()
        {
            const string sql = @"
            DELETE FROM dbo.ImportProductStaging;
            DELETE FROM dbo.ImportJobs;
            DELETE FROM dbo.ReportJobs;
            DELETE FROM dbo.Notifications;
            DELETE FROM dbo.ProductHistories;
            DELETE FROM dbo.Products;
            DELETE FROM dbo.RefreshTokens;
            DELETE FROM dbo.AspNetUserRoles;
            DELETE FROM dbo.AspNetUserClaims;
            DELETE FROM dbo.AspNetUserLogins;
            DELETE FROM dbo.AspNetUserTokens;
            DELETE FROM dbo.AspNetUsers;";

            await using var connection = new SqlConnection(ConnectionString);
            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

    }
}
