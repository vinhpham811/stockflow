using StockFlow.Domain.Common;
using StockFlow.Domain.Exceptions;
using StockFlow.Domain.Products.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products
{
    /// <summary>
    /// Aggregate Root: Sản phẩm.
    /// Mọi thay đổi phải đi qua các method nghiệp vụ bên dưới.
    /// </summary>
    public class Product : AggregateRoot
    {
        public const int NameMaxLength = 200;
        public const int CategoryMaxLength = 100;
        public Sku Sku { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;
        public string? Category { get; private set; }
        public Money Price { get; private set; } = null!;
        public int Quantity { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;
        public DateTime? UpdatedAt { get; private set; }
        public string? UpdatedBy { get; private set; }
        // Dành cho EF Core
        private Product()
        {

        }

        public static Product Create(Sku sku, string name, string? category, Money price, int initialQuantity, string createdBy)
        {
            ValidateName(name);
            ValidateCategory(category);

            if (initialQuantity < 0)
            {
                throw new DomainException("Initial quantity must be greater than or equal to 0.");
            }

            var product = new Product
            {
                Sku = sku,
                Name = name.Trim(),
                Category = NormalizeCategory(category),
                Price = price,
                Quantity = initialQuantity,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            product.AddDomainEvent(new ProductCreatedEvent(sku.Value, product.Name, initialQuantity, createdBy));

            return product;
        }

        public void UpdateDetails(string name, string? category, Money price, string updatedBy)
        {
            EnsureNotDeleted();
            ValidateName(name);
            ValidateCategory(category);

            var oldPrice = Price;
            Name = name.Trim();
            Category = NormalizeCategory(category);
            Price = price;

            MarkAsUpdated(updatedBy);
            // Chỉ phát event khi giá THỰC SỰ thay đổi
            if(oldPrice != price)
            {
                AddDomainEvent(new ProductPriceChangedEvent(Id, Sku.Value, oldPrice.Amount, price.Amount, updatedBy));
            }
        }
        /// <summary>Nhập kho.</summary>
        public void ReceiveStock(int amount, string updatedBy)
        {
            EnsureNotDeleted();
            if (amount <= 0)
                throw new DomainException("Stock-in amount must be greater than 0.");
            var oldQuantity = Quantity;
            Quantity += amount;
            MarkAsUpdated(updatedBy);
            AddDomainEvent(new ProductStockChangedEvent(Id, Sku.Value, oldQuantity, Quantity, StockMovementType.StockIn, updatedBy));
        }
        /// <summary>Xuất kho. Không cho xuất quá số lượng đang có.</summary>
        public void IssueStock(int amount, string updatedBy)
        {
            EnsureNotDeleted();

            if (amount <= 0)
                throw new DomainException("Stock-out amount must be greater than 0.");

            if (amount > Quantity)
                throw new DomainException($"Not enough stock. Available: {Quantity}, requested: {amount}.");

            var oldQuantity = Quantity;
            Quantity -= amount;
            MarkAsUpdated(updatedBy);

            AddDomainEvent(new ProductStockChangedEvent(Id, Sku.Value, oldQuantity, Quantity, StockMovementType.StockOut, updatedBy));
        }
        public void Delete(string deletedBy)
        {
            EnsureNotDeleted();

            IsDeleted = true;
            MarkAsUpdated(deletedBy);
        }
        private void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }
        private void EnsureNotDeleted()
        {
            if (IsDeleted)
                throw new DomainException("Product has been deleted.");
        }
        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Name is required.");

            if (name.Trim().Length > NameMaxLength)
                throw new DomainException($"Name must not exceed {NameMaxLength} characters.");
        }
        private static void ValidateCategory(string? category)
        {
            if (category != null && category.Trim().Length > CategoryMaxLength)
                throw new DomainException($"Category must not exceed {CategoryMaxLength} characters.");
        }
        private static string? NormalizeCategory(string? category)
        {
            return string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        }
    }
}
