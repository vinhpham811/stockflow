using StockFlow.Domain.Exceptions;
using StockFlow.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Tests.Products
{
    public class SkuTests
    {
        public void Create_ShouldTrimAndUpperCase()
        {
            var sku = Sku.Create("  lap-001 ");
            Assert.Equal("LAP-001", sku.Value);
            Assert.Equal("LAP-001", sku.ToString());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyValue_ShouldThrow(string? value) {
            Assert.Throws<DomainException>(() => Sku.Create(value));
        }
        [Fact]
        public void Create_WithTooLongValue_ShouldThrow()
        {
            var tooLong = new string('A', Sku.MaxLength + 1);

            Assert.Throws<DomainException>(() => Sku.Create(tooLong));
        }

        [Fact]
        public void TwoSkus_WithSameValue_ShouldBeEqual()
        {
            Assert.Equal(Sku.Create("lap-001"), Sku.Create("LAP-001"));
        }
    }
}
