using StockFlow.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Lớp cha cho mọi integration test: tự xóa sạch dữ liệu TRƯỚC mỗi test.
    /// </summary>
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected DatabaseFixture Fixture { get; }
        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            Fixture = fixture;
        }

        public Task InitializeAsync()
        {
            return Fixture.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
        /// <summary>Tạo 1 user trong AspNetUsers (cần cho khóa ngoại của RefreshTokens).</summary>
        protected async Task<string> CreateUserAsync(string email = "user@test.com")
        {
            await using var context = Fixture.CreateDBContext();
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                NormalizedUserName = email.ToUpperInvariant(),
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                FullName = "Test User",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user.Id;
        }
        
    }
}
