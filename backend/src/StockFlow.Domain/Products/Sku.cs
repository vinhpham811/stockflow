using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products
{
    /// <summary>
    /// Value Object: mã sản phẩm.
    /// - Bất biến (không có setter)
    /// - So sánh theo giá trị (record tự làm việc này)
    /// - Luôn hợp lệ: không thể tạo ra một Sku sai
    /// </summary>
    public sealed record Sku
    {
        public const int MaxLength = 50;
        public string Value { get; }
        private Sku (string value)
        {
            Value = value;
        }
        public static Sku Create (string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new DomainException("SKU is required.");
            }
            var normalized = value.Trim().ToUpperInvariant();
            if (normalized.Length > MaxLength)
                throw new DomainException($"SKU must not exceed {MaxLength} characters.");
            return new Sku(normalized);
        }
        public override string ToString() => Value;
        //public override string ToString()
        //{
        //    return base.ToString();
        //}
    }
}
