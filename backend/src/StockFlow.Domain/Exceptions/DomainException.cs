using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Domain.Exceptions
{
    /// <summary>
    /// Lỗi vi phạm rule nghiệp vụ (ví dụ: giá âm, thiếu SKU).
    /// </summary>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}
