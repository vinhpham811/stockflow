using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        public async Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
        {
            // SKU là duy nhất kể cả với sản phẩm đã xóa mềm → bỏ qua query filter
            return await _context.Products
                .IgnoreQueryFilters()
                .AnyAsync(p => p.Sku == sku, cancellationToken);
        }
        public void Add(Product product)
        {
            _context.Products.Add(product);
        }
    }
}
