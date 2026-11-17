using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StockFlow.Domain.Entities
{
    public class Product
    {
        public const int SkuMaxLength = 50;
        public const int NameMaxLength = 200;
        public const int categoryMaxLength = 100;
        public int Id { get; private set; }
        public string Sku { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string? Category { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public DateTime? UpdatedAt { get; private set; }
        public string? UpdatedBy { get; private set; }

        // Constructor rỗng cho EF Core
        private Product() { }
        public static Product Create(string sku, string name, string? category, decimal price, int quantity, string createdBy)
        {
            ValidateSku(sku);
            ValidateName(name);
            ValidatePrice(price);
            ValidateQuantity(quantity);
            return new Product
            {
                Sku = sku.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim(),
                Price = price,
                Quantity = quantity,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
        }
        public void UpdateDetails(string name, string? category, decimal price, string updatedBy)
        {
            ValidateName(name);
            ValidatePrice(price);
            Name = name.Trim();
            Category = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
            Price = price;
            MarkAsUpdated(updatedBy);
        }
        public void ChangeQuantity(int quantity, string updatedBy)
        {
            ValidateQuantity(quantity);

            Quantity = quantity;
            MarkAsUpdated(updatedBy);
        }

        public void MarkAsDeleted(string updatedBy)
        {
            IsDeleted = true;
            MarkAsUpdated(updatedBy);
        }
        private void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
        private static void ValidateSku(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
            {
                throw new DomainException("SKU is required.");
            }
            if(sku.Trim().Length > SkuMaxLength)
            {
                throw new DomainException($"SKU cannot exceed {SkuMaxLength} characters.");
            }
        }
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required.");

            if (name.Trim().Length > NameMaxLength)
                throw new DomainException($"Name must not exceed {NameMaxLength} characters.");
        }

        private static void ValidatePrice(decimal price)
        {
            if (price < 0)
                throw new DomainException("Price must be greater than or equal to 0.");
        }

        private static void ValidateQuantity(int quantity)
        {
            if (quantity < 0)
                throw new DomainException("Quantity must be greater than or equal to 0.");
        }
    }
}
