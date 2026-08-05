using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.EfCore.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class SyncStateConfiguration : IEntityTypeConfiguration<SyncState>
    {
        public void Configure(EntityTypeBuilder<SyncState> builder)
        {
            builder.ToTable("SyncStates");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.EntityType)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(s => s.EntityType)
                .IsUnique();

            builder.Property(s => s.LastSyncedAt);

            builder.Property(s => s.LastFullReconciliationAt);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(s => s.LastError)
                .HasMaxLength(2048);

            builder.Property(s => s.UpdatedAt)
                .IsRequired();
        }
    }
}
