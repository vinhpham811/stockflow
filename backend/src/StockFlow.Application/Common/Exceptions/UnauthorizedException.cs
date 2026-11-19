using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Application.Common.Exceptions
{
    /// <summary>Chưa đăng nhập hoặc thông tin đăng nhập sai → HTTP 401.</summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }
}
