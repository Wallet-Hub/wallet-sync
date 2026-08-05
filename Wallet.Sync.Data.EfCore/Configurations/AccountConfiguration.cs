using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.ExternalId)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(a => a.ExternalId)
                .IsUnique();

            builder.Property(a => a.CurrencyCode)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(a => a.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.Type)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(a => a.Color)
                .HasMaxLength(32);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            builder.Property(a => a.UpdatedAt)
                .IsRequired();

            builder.Property<bool>("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(a => !EF.Property<bool>(a, "IsDeleted"));
        }
    }
}
