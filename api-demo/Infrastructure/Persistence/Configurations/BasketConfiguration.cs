using api_demo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace api_demo.Infrastructure.Persistence.Configurations;

public class BasketConfiguration : IEntityTypeConfiguration<Basket>
{
    public void Configure(EntityTypeBuilder<Basket> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();

        builder.HasMany(b => b.Items)
            .WithOne(i => i.Basket)
            .HasForeignKey(i => i.BasketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}