using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class ExpenseDocumentProviderConfiguration : IEntityTypeConfiguration<ExpenseDocumentProvider>
    {
        public void Configure(EntityTypeBuilder<ExpenseDocumentProvider> builder)
        {
            builder.ToTable("ExpenseDocumentProviders");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(p => p.Name)
                .IsUnique();
        }
    }
}
