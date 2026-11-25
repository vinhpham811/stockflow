using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            // Bảng Products đã được tạo bằng script SQL (Giai đoạn 1)
            // => KHÔNG để EF Migration tạo lại bảng này
            builder.ToTable("Products", table => table.ExcludeFromMigrations());

            builder.HasKey(p => p.Id);

            // Value Object Sku <-> cột NVARCHAR
            builder.Property(p => p.Sku)
                .HasConversion(sku => sku.Value, value=>Sku.Create(value))
                .HasMaxLength(Sku.MaxLength)
                .IsRequired();

            // Value Object Money <-> cột DECIMAL(18,2)
            builder.Property(p => p.Price)
                .HasConversion(price => price.Amount, value => Money.Create(value))
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
            builder.Property(p => p.Category).HasMaxLength(Product.CategoryMaxLength);
            
            builder.Property(p => p.CreatedBy).HasMaxLength(256).IsRequired();
            builder.Property(p => p.UpdatedBy).HasMaxLength(256);

            // Domain event không lưu xuống DB
            builder.Ignore(p => p.DomainEvents);

            // Mọi câu query tự động bỏ qua sản phẩm đã xóa mềm
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
