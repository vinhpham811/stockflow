using Microsoft.Extensions.Logging;
using StockFlow.Application.Common.Events;
using StockFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Events
{
    /// <summary>
    /// Gửi từng domain event tới các handler phù hợp.
    /// </summary>
    public class DomainEventDispatcher
    {
        private readonly IEnumerable<IDomainEventHandler> _handlers;
        private readonly ILogger<DomainEventDispatcher> _logger;
        public DomainEventDispatcher(IEnumerable<IDomainEventHandler> handlers, ILogger<DomainEventDispatcher> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
        {
            foreach (var domainEvent in domainEvents)
            {
                foreach (var handler in _handlers)
                {
                    if (!handler.CanHandle(domainEvent))
                    {
                        continue;
                    }
                    try
                    {
                        await handler.HandleAsync(domainEvent, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Dữ liệu đã lưu thành công → lỗi handler không được làm hỏng request,
                        // chỉ ghi log để theo dõi.
                        _logger.LogError(ex, "Failed to handle domain Event {EventType}", domainEvent.GetType().Name);
                    }
                }
            }
        }
    }
}
