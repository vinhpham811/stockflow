using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Tests.Entities
{
    public class RefreshTokenTests
    {
        [Fact]
        public void Create_WithValidData_ShouldBeActive()
        {
            var token = RefreshToken.Create("user-1", "hash-abc", DateTime.UtcNow.AddDays(7));

            Assert.Equal("user-1", token.UserId);
            Assert.Equal("hash-abc", token.TokenHash);
            Assert.True(token.IsActive);
            Assert.Null(token.RevokedAt);
        }

        [Theory]
        [InlineData("", "hash")]
        [InlineData("user-1", "")]
        public void Create_WithMissingData_ShouldThrowDomainException(string userId, string tokenHash)
        {
            Assert.Throws<DomainException>(() => RefreshToken.Create(userId, tokenHash, DateTime.UtcNow.AddDays(7)));
        }

        [Fact]
        public void Create_WithPastExpiry_ShouldThrowDomainException()
        {
            Assert.Throws<DomainException>(() => RefreshToken.Create("user-1", "hash", DateTime.UtcNow.AddMinutes(-1)));
        }

        [Fact]
        public void Revoke_ShouldMakeTokenInactive()
        {
            var token = RefreshToken.Create("user-1", "hash", DateTime.UtcNow.AddDays(7));

            token.Revoke();

            Assert.False(token.IsActive);
            Assert.NotNull(token.RevokedAt);
        }

        [Fact]
        public void Revoke_CalledTwice_ShouldKeepFirstRevokedTime() {
            var token = RefreshToken.Create("user-1", "hash", DateTime.UtcNow.AddDays(7));
            token.Revoke();
            var FirstRevokedAt = token.RevokedAt;
            token.Revoke();
            Assert.Equal(FirstRevokedAt, token.RevokedAt);
        }
    }
}
