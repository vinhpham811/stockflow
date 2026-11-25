using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products.Events
{
    /// <summary>
    /// Sản phẩm mới được tạo. Dùng Sku (mã nghiệp vụ duy nhất) vì lúc tạo chưa có Id từ DB.
    /// </summary>
    public sealed record ProductCreatedEvent(
        string Sku,
        String Name,
        int InitialQuantity,
        string CreatedBy): IDomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.Now;
    }
}
