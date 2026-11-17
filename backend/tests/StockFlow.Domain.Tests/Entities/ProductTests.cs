using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Tests.Entities
{
    public  class ProductTests
    {
        [Fact]
        public void Create_WithValidData_ShouldCreateProduct() { 
            var product= Product.Create(" lap-001 ", " Laptop Dell ", "Laptop", 1000m, 5, "admin");
            Assert.Equal("LAP-001", product.Sku);
            Assert.Equal("Laptop Dell", product.Name);
            Assert.Equal("Laptop", product.Category);
            Assert.Equal(1000m, product.Price);
            Assert.Equal(5, product.Quantity);
            Assert.Equal("admin", product.CreatedBy);
            Assert.False(product.IsDeleted);
        }

        [Fact]
        public void Create_WithEmptyCategory_ShouldSetCategoryToNull()
        {
            var product = Product.Create("SKU-1", "Name", "   ", 100m, 1, "admin");

            Assert.Null(product.Category);
        }

        [Theory]
        [InlineData("", "Name", 100, 1)]
        [InlineData("SKU-1", "", 100, 1)]
        [InlineData("SKU-1", "Name", -1, 1)]
        [InlineData("SKU-1", "Name", 100, -1)]

        public void Create_WithInvalidData_ShouldThrowDomainException(string sku, string name, int price, int quantity)
        {
            Assert.Throws<DomainException>(() => Product.Create(sku, name, null, price, quantity, "admin"));
        }

        [Fact]
        public void Create_WithTooLongSku_ShouldThrowDomainException()
        {
            var longSku = new string('A', Product.SkuMaxLength + 1);

            Assert.Throws<DomainException>(() => Product.Create(longSku, "Name", null, 100m, 1, "admin"));
        }
        [Fact]
        public void Create_WithTooLongName_ShouldThrowDomainException()
        {
            var longName = new string('A', Product.NameMaxLength + 1);

            Assert.Throws<DomainException>(() => Product.Create("SKU-1", longName, null, 100m, 1, "admin"));
        }
        [Fact]
        public void UpdateDetails_WithValidData_ShouldUpdateAndSetAuditInfo()
        {
            var product = Product.Create("SKU-1", "Old name", "Old", 100m, 1, "admin");

            product.UpdateDetails("New name", "New", 200m, "staff");

            Assert.Equal("New name", product.Name);
            Assert.Equal("New", product.Category);
            Assert.Equal(200m, product.Price);
            Assert.Equal("staff", product.UpdatedBy);
            Assert.NotNull(product.UpdatedAt);
        }
        [Fact]
        public void ChangeQuantity_WithValidValue_ShouldUpdateQuantity()
        {
            var product = Product.Create("SKU-1", "Name", null, 100m, 1, "admin");

            product.ChangeQuantity(50, "staff");

            Assert.Equal(50, product.Quantity);
            Assert.Equal("staff", product.UpdatedBy);
        }
        [Fact]
        public void ChangeQuantity_WithNegativeValue_ShouldThrowDomainException()
        {
            var product = Product.Create("SKU-1", "Name", null, 100m, 1, "admin");

            Assert.Throws<DomainException>(() => product.ChangeQuantity(-5, "staff"));
        }
        [Fact]
        public void MarkAsDeleted_ShouldSetIsDeletedToTrue()
        {
            var product = Product.Create("SKU-1", "Name", null, 100m, 1, "admin");

            product.MarkAsDeleted("admin");

            Assert.True(product.IsDeleted);
            Assert.Equal("admin", product.UpdatedBy);
        }
    }
}
