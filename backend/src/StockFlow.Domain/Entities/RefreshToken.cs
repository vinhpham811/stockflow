using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Domain.Entities
{
    /// <summary>
    /// Refresh token lưu trong DB. Chỉ lưu bản HASH, không lưu token gốc
    /// để nếu DB bị lộ thì kẻ gian cũng không dùng được token.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; private set; }
        public string UserId { get; private set; } = string.Empty;
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
        //public bool IsActive
        //{
        //    get
        //    {
        //        return RevokedAt == null && DateTime.UtcNow < ExpiresAt;
        //    }
        //}
        private RefreshToken() { }
        public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new DomainException("UserId is required.");
            }
            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new DomainException("TokenHash is required.");
            }
            if (expiresAt <= DateTime.UtcNow)
            {
                throw new DomainException("ExpiresAt must be a future date.");
            }
            return new RefreshToken
            {
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            };
        }
        public void Revoke()
        {
            if (RevokedAt == null)
            {
                RevokedAt = DateTime.UtcNow;
            }
        }
    }
}
