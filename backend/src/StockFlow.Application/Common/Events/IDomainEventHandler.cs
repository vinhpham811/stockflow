using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common.Events
{
    public interface IDomainEventHandler
    {
        bool CanHandle(IDomainEvent domainEvent);
        Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken);
    }
}
