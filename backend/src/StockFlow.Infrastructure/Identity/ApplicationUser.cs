using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Identity
{
    /// <summary>
    /// User của hệ thống, kế thừa IdentityUser (đã có Email, PasswordHash...).
    /// </summary>
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
