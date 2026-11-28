using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.IntegrationTests.Infrastructure;
using StockFlow.Infrastructure.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.IntegrationTests.Repositories
{
    [Collection(DatabaseCollection.Name)]
    public class RefeshTokenRepositoryTests : IntegrationTestBase
    {
        public RefeshTokenRepositoryTests(DatabaseFixture fixture) : base(fixture)
        {

        }
        [Fact]
        public async Task AddAsync_ThenSaveChanges_ShouldPersistToDatabase()
        {
            var userId = await CreateUserAsync();
            
            //Act - dùng DbCOntext #1
            await using (var context = Fixture.CreateDBContext())
            {
                var repository = new RefreshTokenRepository(context);
                await repository.AddAsync(RefreshToken.Create(userId, "hash-1", DateTime.UtcNow.AddDays(7)));
                await repository.SaveChangesAsync();
            }

            // Assert – dùng DbContext #2 MỚI → đọc thật từ DB
            await using var verifyContext = Fixture.CreateDBContext();
            var saved = await verifyContext.RefreshTokens.SingleAsync();

            Assert.True(saved.Id > 0);
            Assert.Equal(userId, saved.UserId);
            Assert.Equal("hash-1", saved.TokenHash);
            Assert.Null(saved.RevokedAt);
        }

        [Fact]
        public async Task GetByTokenHashAsync_WithExistingHash_ShouldReturnToken()
        {
            var userId = await CreateUserAsync();
            await SeedTokenAsync(userId, "hash-abc");

            await using var context = Fixture.CreateDBContext();
            var repository = new RefreshTokenRepository(context);

            var result = await repository.GetByTokenHashAsync("hash-abc");

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
        }
        [Fact]
        public async Task GetByTokenHashAsync_WithUnknownHash_ShouldReturnNull()
        {
            var userId = await CreateUserAsync();
            await SeedTokenAsync(userId, "hash-abc");

            await using var context = Fixture.CreateDBContext();
            var repository = new RefreshTokenRepository(context);

            var result = await repository.GetByTokenHashAsync("hash-khong-ton-tai");

            Assert.Null(result);
        }

        [Fact]
        public async Task Revoke_ThenSaveChanges_ShouldUpdateRevokedAtInDatabase()
        {
            var userId = await CreateUserAsync();
            await SeedTokenAsync(userId, "hash-revoke");

            // Act: load (có tracking) → Revoke() → SaveChanges (KHÔNG gọi Update)
            await using (var context = Fixture.CreateDBContext())
            {
                var repository = new RefreshTokenRepository(context);
                var token = await repository.GetByTokenHashAsync("hash-revoke");
                token!.Revoke();
                await repository.SaveChangesAsync();
            }

            // Assert: đọc lại bằng DbContext mới
            await using var verifyContext = Fixture.CreateDBContext();
            var saved = await verifyContext.RefreshTokens.SingleAsync(t => t.TokenHash == "hash-revoke");

            Assert.NotNull(saved.RevokedAt);
            Assert.False(saved.IsActive);

        }

        [Fact]
        public async Task AddAsync_WithDuplicateHash_ShouldThrowBecauseOfUniqueIndex()
        {
            var userId = await CreateUserAsync();
            await SeedTokenAsync(userId, "hash-duplicate");

            await using var context = Fixture.CreateDBContext();
            var repository = new RefreshTokenRepository(context);
            await repository.AddAsync(RefreshToken.Create(userId, "hash-duplicate", DateTime.UtcNow.AddDays(7)));

            await Assert.ThrowsAsync<DbUpdateException>(() => repository.SaveChangesAsync());
        }

        private async Task SeedTokenAsync(string userId, string tokenHash)
        {
            await using var context = Fixture.CreateDBContext();
            context.RefreshTokens.Add(RefreshToken.Create(userId, tokenHash, DateTime.UtcNow.AddDays(7)));
            await context.SaveChangesAsync();
        }
    }
}
