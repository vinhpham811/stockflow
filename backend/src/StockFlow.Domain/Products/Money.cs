using StockFlow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products
{
    /// <summary>
    /// Value Object: số tiền (hệ thống chỉ dùng 1 loại tiền là VND).
    /// </summary>
    public sealed record Money
    {
        public decimal Amount { get; }
        private Money(decimal amount)
        {
            Amount = amount;
        }
        public static Money Zero => new(0);
        //public static Money Zero
        //{
        //    get { return new Money(0); }
        //}
        public static Money Create(decimal amount)
        {
            if (amount < 0)
                throw new DomainException("Price must be greater than or equal to 0.");

            if (decimal.Round(amount, 2) != amount)
                throw new DomainException("Price must have at most 2 decimal places.");

            return new Money(amount);
        }

    }
}
