using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.ExternalId)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(l => l.ExternalId)
                .IsUnique();

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(l => l.Color)
                .HasMaxLength(32);

            builder.Property(l => l.CreatedAt)
                .IsRequired();

            builder.Property(l => l.UpdatedAt)
                .IsRequired();

            builder.Property<bool>("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasQueryFilter(l => !EF.Property<bool>(l, "IsDeleted"));
        }
    }
}
