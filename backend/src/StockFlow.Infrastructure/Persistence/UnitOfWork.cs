using StockFlow.Domain.Common;
using StockFlow.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly DomainEventDispatcher _dispatcher;

        public UnitOfWork(AppDbContext context, DomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 1. Gom các aggregate có domain event
            var aggregates = _context.ChangeTracker
                .Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregate => aggregate.DomainEvents.Count > 0)
                .ToList();

            var domainEvents = aggregates
                .SelectMany(aggregate => aggregate.DomainEvents)
                .ToList();

            // 2. Lưu DB (1 transaction)
            var result = await _context.SaveChangesAsync(cancellationToken);

            // 3. Xóa event khỏi aggregate để không bị gửi lại
            foreach(var aggregate in aggregates)
            {
                aggregate.ClearDomainEvents();
            }

            // 4. Chỉ phát event SAU KHI lưu thành công
            await _dispatcher.DispatchAsync(domainEvents, cancellationToken);

            return result;
        }
    }
}
