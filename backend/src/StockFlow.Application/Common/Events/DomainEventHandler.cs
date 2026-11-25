using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common.Events
{
    /// <summary>
    /// Lớp cha giúp viết handler gọn hơn: chỉ cần override Handle với đúng kiểu event.
    /// Ví dụ: class X : DomainEventHandler<ProductCreatedEvent>
    /// </summary>
    public abstract class DomainEventHandler<TEvent>: IDomainEventHandler
        where TEvent: IDomainEvent
    {
        public bool CanHandle(IDomainEvent domainEvent)
        {
            return domainEvent is TEvent;
        }
        public Task HandleAsync(IDomainEvent domainEvent, CancellationToken cancellationToken) {
            return Handle((TEvent)domainEvent, cancellationToken);
        }
        protected abstract Task Handle(TEvent domainEvent, CancellationToken cancellationToken);
    }
}
