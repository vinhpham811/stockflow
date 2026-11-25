using StockFlow.Domain.Exceptions;
using StockFlow.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Tests.Products
{
    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidAmount_ShouldKeepAmount()
        {
            var money = Money.Create(1500.50m);

            Assert.Equal(1500.50m, money.Amount);
        }
        [Fact]
        public void Create_WithNegativeAmount_ShouldThrow()
        {
            Assert.Throws<DomainException>(() => Money.Create(-1m));
        }
        [Fact]
        public void Create_WithMoreThanTwoDecimals_ShouldThrow()
        {
            Assert.Throws<DomainException>(() => Money.Create(10.123m));
        }

        [Fact]
        public void Zero_ShouldHaveZeroAmount()
        {
            Assert.Equal(0m, Money.Zero.Amount);
        }
        [Fact]
        public void TwoMoneys_WithSameAmount_ShouldBeEqual()
        {
            Assert.Equal(Money.Create(100m), Money.Create(100m));
            Assert.NotEqual(Money.Create(100m), Money.Create(200m));
        }
    }
}
