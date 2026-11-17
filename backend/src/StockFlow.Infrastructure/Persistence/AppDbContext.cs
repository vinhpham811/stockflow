using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence
{
    public class AppDbContext: IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        //public DbSet<Product> Products { get; set; } = null!;
        //public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Bắt buộc gọi base để Identity tạo cấu hình cho các bảng AspNet *
            base.OnModelCreating(builder);
            // Tự động áp dụng tất cả class IEntityTypeConfiguration trong project này
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }
    }
}
