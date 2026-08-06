using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class ExternalProviderAccountConfiguration : IEntityTypeConfiguration<ExternalProviderAccount>
    {
        public void Configure(EntityTypeBuilder<ExternalProviderAccount> builder)
        {
            builder.ToTable("ExternalProviderAccounts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Reference)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(a => a.Description)
                .HasMaxLength(1024);

            builder.HasIndex(a => new { a.ProviderId, a.Reference })
                .IsUnique();

            builder.HasOne(a => a.Provider)
                .WithMany()
                .HasForeignKey(a => a.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
