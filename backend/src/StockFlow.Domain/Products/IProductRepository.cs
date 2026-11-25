using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Products
{
    /// <summary>
    /// Repository cho aggregate Product - CHỈ phục vụ phía GHI.
    /// Không có hàm Update: EF tự theo dõi thay đổi, Unit of Work sẽ lưu.
    /// Phía ĐỌC (tìm kiếm, phân trang) dùng query service riêng gọi stored procedure.
    /// </summary>
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
        void Add(Product product);
    }
}
