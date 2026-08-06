using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.ExternalId)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(c => c.ExternalId)
                .IsUnique();

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(c => c.GroupId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(c => c.GroupName)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(c => c.Color)
                .HasMaxLength(32);

            builder.Property(c => c.ParentExternalId)
                .HasMaxLength(128);

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt)
                .IsRequired();

            builder.Property<bool>("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(c => c.Parent)
                .WithMany()
                .HasForeignKey(c => c.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(c => !EF.Property<bool>(c, "IsDeleted"));
        }
    }
}
