using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products.Events
{
    public sealed record ProductStockChangedEvent(
        int ProductId,
        string Sku,
        int OldQuantity,
        int NewQuantity,
        StockMovementType MovementType,
        string ChangedBy): IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
