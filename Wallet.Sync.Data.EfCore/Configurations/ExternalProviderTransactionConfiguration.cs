using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class ExternalProviderTransactionConfiguration : IEntityTypeConfiguration<ExternalProviderTransaction>
    {
        public void Configure(EntityTypeBuilder<ExternalProviderTransaction> builder)
        {
            builder.ToTable("ExternalProviderTransactions");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.TransactionId)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(t => t.Description)
                .HasMaxLength(1024);

            // One-to-one: a Record has at most one associated ExternalProviderTransaction.
            builder.HasIndex(t => t.RecordId)
                .IsUnique();

            builder.HasOne(t => t.Record)
                .WithOne()
                .HasForeignKey<ExternalProviderTransaction>(t => t.RecordId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.ExternalProviderAccount)
                .WithMany()
                .HasForeignKey(t => t.ExternalProviderAccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
