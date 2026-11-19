using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common.Exceptions
{
    /// <summary>Dữ liệu gửi lên không hợp lệ → HTTP 400.</summary>
    public class BadRequestException: Exception
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}
