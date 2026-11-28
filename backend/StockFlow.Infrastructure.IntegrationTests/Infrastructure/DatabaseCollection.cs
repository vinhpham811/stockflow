using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Các test class gắn [Collection(DatabaseCollection.Name)] sẽ:
    /// - Dùng CHUNG 1 DatabaseFixture (chỉ bật container 1 lần)
    /// - Chạy LẦN LƯỢT (không song song) để không giẫm dữ liệu của nhau
    /// </summary>
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        public const string Name = "Database";
    }
}
