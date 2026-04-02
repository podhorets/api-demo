using api_demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_demo.Infrastructure.Persistence.Configurations;

public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.ItemNo).IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasPrecision(10, 2).IsRequired();
        
        builder.HasIndex(i => i.BasketId);
        builder.HasIndex(i => new { i.BasketId, i.ItemNo }).IsUnique();
    }
}