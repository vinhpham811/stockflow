using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products.Events
{
    public sealed record ProductPriceChangedEvent(
        int ProductId,
        string Sku,
        decimal OldPrice,
        decimal NewPrice,
        string ChangedBy): IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
