using StockFlow.Domain.Exceptions;
using StockFlow.Domain.Products;
using StockFlow.Domain.Products.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Tests.Products
{
    public class ProductTests
    {
        private static Product CreateProduct(int quantity = 10, decimal price = 1000m)
        {
            return Product.Create(Sku.Create("SKU-1"), "Laptop", "Computer", Money.Create(price), quantity, "admin");
        }

        // ========== CREATE ==========
        [Fact]
        public void Create_WithValidData_ShouldSetPropertiesAndRaiseCreatedEvent()
        {
            var product = Product.Create(Sku.Create("sku-1"), " Laptop ", " Computer ", Money.Create(1000m), 5, "admin");

            Assert.Equal("SKU-1", product.Sku.Value);
            Assert.Equal("Laptop", product.Name);
            Assert.Equal("Computer", product.Category);
            Assert.Equal(1000m, product.Price.Amount);
            Assert.Equal(5, product.Quantity);
            Assert.Equal("admin", product.CreatedBy);
            Assert.False(product.IsDeleted);

            var domainEvent = Assert.IsType<ProductCreatedEvent>(Assert.Single(product.DomainEvents));
            Assert.Equal("SKU-1", domainEvent.Sku);
            Assert.Equal(5, domainEvent.InitialQuantity);
        }
        [Fact]
        public void Create_WithBlankCategory_ShouldSetCategoryToNull()
        {
            var product = Product.Create(Sku.Create("SKU-1"), "Laptop", "   ", Money.Zero, 0, "admin");

            Assert.Null(product.Category);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_ShouldThrow(string name)
        {
            Assert.Throws<DomainException>(() => Product.Create(Sku.Create("SKU-1"), name, null, Money.Zero, 0, "admin"));
        }

        [Fact]
        public void Create_WithTooLongName_ShouldThrow()
        {
            var name = new string('A', Product.NameMaxLength + 1);

            Assert.Throws<DomainException>(() => Product.Create(Sku.Create("SKU-1"), name, null, Money.Zero, 0, "admin"));
        }

        [Fact]
        public void Create_WithTooLongCategory_ShouldThrow()
        {
            var category = new string('A', Product.CategoryMaxLength + 1);

            Assert.Throws<DomainException>(() => Product.Create(Sku.Create("SKU-1"), "Laptop", category, Money.Zero, 0, "admin"));
        }

        [Fact]
        public void Create_WithNegativeQuantity_ShouldThrow()
        {
            Assert.Throws<DomainException>(() => Product.Create(Sku.Create("SKU-1"), "Laptop", null, Money.Zero, -1, "admin"));
        }

        // ========== UPDATE DETAILS ==========
        [Fact]
        public void UpdateDetails_WithNewPrice_ShouldRaisePriceChangedEvent()
        {
            var product = CreateProduct(price: 1000m);
            product.ClearDomainEvents(); // tránh bị "nhiễu" bởi ProductCreatedEvent vốn được thêm vào danh sách ngay lúc gọi Product.Create(...).

            product.UpdateDetails("Laptop Pro", "Computer", Money.Create(1200m), "staff");

            Assert.Equal("Laptop Pro", product.Name);
            Assert.Equal(1200m, product.Price.Amount);
            Assert.Equal("staff", product.UpdatedBy);
            Assert.NotNull(product.UpdatedAt);

            var domainEvent = Assert.IsType<ProductPriceChangedEvent>(Assert.Single(product.DomainEvents));
            Assert.Equal(1000m, domainEvent.OldPrice);
            Assert.Equal(1200m, domainEvent.NewPrice);
        }

        [Fact]
        public void UpdateDetails_WithSamePrice_ShouldNotRaiseEvent()
        {
            var product = CreateProduct(price: 1000m);
            product.ClearDomainEvents();  // tránh bị "nhiễu" bởi ProductCreatedEvent vốn được thêm vào danh sách ngay lúc gọi Product.Create(...).

            product.UpdateDetails("New name", null, Money.Create(1000m), "staff");

            Assert.Empty(product.DomainEvents);
        }
        // ========== RECEIVE STOCK (NHẬP KHO) ==========

        [Fact]
        public void ReceiveStock_ShouldIncreaseQuantityAndRaiseEvent()
        {
            var product = CreateProduct(quantity: 10);
            product.ClearDomainEvents();

            product.ReceiveStock(5, "staff");

            Assert.Equal(15, product.Quantity);
            var domainEvent = Assert.IsType<ProductStockChangedEvent>(Assert.Single(product.DomainEvents));
            Assert.Equal(10, domainEvent.OldQuantity);
            Assert.Equal(15, domainEvent.NewQuantity);
            Assert.Equal(StockMovementType.StockIn, domainEvent.MovementType);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public void ReceiveStock_WithInvalidAmount_ShouldThrow(int amount)
        {
            var product = CreateProduct();

            Assert.Throws<DomainException>(() => product.ReceiveStock(amount, "staff"));
        }

        // ========== ISSUE STOCK (XUẤT KHO) ==========

        [Fact]
        public void IssueStock_ShouldDecreaseQuantityAndRaiseEvent()
        {
            var product = CreateProduct(quantity: 10);
            product.ClearDomainEvents();

            product.IssueStock(4, "staff");

            Assert.Equal(6, product.Quantity);
            var domainEvent = Assert.IsType<ProductStockChangedEvent>(Assert.Single(product.DomainEvents));
            Assert.Equal(StockMovementType.StockOut, domainEvent.MovementType);
        }

        [Fact]
        public void IssueStock_MoreThanAvailable_ShouldThrow()
        {
            var product = CreateProduct(quantity: 3);

            Assert.Throws<DomainException>(() => product.IssueStock(4, "staff"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void IssueStock_WithInvalidAmount_ShouldThrow(int amount)
        {
            var product = CreateProduct();

            Assert.Throws<DomainException>(() => product.IssueStock(amount, "staff"));
        }
        // ========== DELETE ==========

        [Fact]
        public void Delete_ShouldMarkProductAsDeleted()
        {
            var product = CreateProduct();

            product.Delete("admin");

            Assert.True(product.IsDeleted);
            Assert.Equal("admin", product.UpdatedBy);
        }

        [Fact]
        public void AnyChange_OnDeletedProduct_ShouldThrow()
        {
            var product = CreateProduct();
            product.Delete("admin");

            Assert.Throws<DomainException>(() => product.ReceiveStock(1, "staff"));
            Assert.Throws<DomainException>(() => product.IssueStock(1, "staff"));
            Assert.Throws<DomainException>(() => product.UpdateDetails("X", null, Money.Zero, "staff"));
            Assert.Throws<DomainException>(() => product.Delete("admin"));
        }

        [Fact]
        public void ClearDomainEvents_ShouldRemoveAllEvents()
        {
            var product = CreateProduct();
            product.ReceiveStock(1, "staff");

            product.ClearDomainEvents();

            Assert.Empty(product.DomainEvents);
        }
    }
}
