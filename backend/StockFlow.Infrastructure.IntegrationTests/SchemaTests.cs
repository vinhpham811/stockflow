using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using StockFlow.Infrastructure.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.IntegrationTests
{
    [Collection(DatabaseCollection.Name)]
    public class SchemaTests : IntegrationTestBase
    {
        public SchemaTests(DatabaseFixture fixture): base(fixture)
        {

        }

        [Fact]
        public async Task Database_ShouldContainAllRequiredTables()
        {
            await using var context = Fixture.CreateDBContext();
            // Với kiểu đơn giản (string), EF Core 8 yêu cầu cột trả về tên là "Value"
            var tables = await context.Database
                .SqlQueryRaw<string>("SELECT name AS Value FROM sys.tables")
                .ToListAsync();

            var expectedTables = new[]
            {
                "Products", "ProductHistories", "Notifications", "ImportJobs",
            "ImportProductStaging", "ReportJobs", "RefreshTokens", "AspNetUsers", "AspNetRoles"
            };

            foreach (var table in expectedTables)
            {
                Assert.Contains(table,tables);
            }

        }

        public async Task Database_ShouldContainAllStorageProcedures()
        {
            await using var context = Fixture.CreateDBContext();

            var procedures = await context.Database
                .SqlQueryRaw<string>("SELECT name AS Value FROM sys.procedures")
                .ToListAsync();

            Assert.Contains("usp_Product_SearchPaged", procedures);
            Assert.Contains("usp_Import_MergeProducts", procedures);
            Assert.Contains("usp_Report_Inventory", procedures);
        }
    }
}
