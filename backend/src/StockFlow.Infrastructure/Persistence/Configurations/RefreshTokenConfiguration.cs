using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockFlow.Domain.Entities;
using StockFlow.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockFlow.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId).HasMaxLength(450).IsRequired();
            builder.Property(r => r.TokenHash).HasMaxLength(200).IsRequired();

            builder.HasIndex(r => r.TokenHash).IsUnique();
            // Khóa ngoại tới bảng AspNetUsers, xóa user thì xóa luôn token
            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // IsActive là property tính toán, không lưu xuống DB
            builder.Ignore(r => r.IsActive);
        }
    }
}
