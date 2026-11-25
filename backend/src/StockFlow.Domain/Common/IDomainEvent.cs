using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Common
{
    /// <summary>
    /// Một sự việc nghiệp vụ ĐÃ xảy ra (đặt tên ở thì quá khứ: Created, Changed...).
    /// </summary>
    public interface IDomainEvent
    {
        DateTime OccurredAt { get; }
    }
}
