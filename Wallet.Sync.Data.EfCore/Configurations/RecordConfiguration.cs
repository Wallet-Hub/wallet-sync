using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class RecordConfiguration : IEntityTypeConfiguration<Record>
    {
        public void Configure(EntityTypeBuilder<Record> builder)
        {
            builder.ToTable("Records");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.ExternalId)
                .IsRequired()
                .HasMaxLength(128);

            builder.HasIndex(r => r.ExternalId)
                .IsUnique();

            builder.Property(r => r.ExternalAccountId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(r => r.ExternalCategoryId)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(r => r.Description)
                .IsRequired()
                .HasMaxLength(1024);

            builder.Property(r => r.RecordDate)
                .IsRequired();

            builder.Property(r => r.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(r => r.RecordState)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.Property(r => r.RecordType)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            builder.Property(r => r.Note)
                .HasMaxLength(2048);

            builder.Property<bool>("IsDeleted")
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasOne(r => r.Account)
                .WithMany()
                .HasForeignKey(r => r.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.ExpenseDocuments)
                .WithOne(e => e.Record)
                .HasForeignKey(e => e.RecordId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(r => r.Labels)
                .WithMany()
                .UsingEntity(j => j.ToTable("RecordLabels"));

            // Account/Category are required navigations that also carry their own
            // shadow "IsDeleted" query filter, so it must be repeated here to avoid
            // EF Core's "required navigation + query filter" warning and to keep
            // filtered Records consistent with their (non-deleted) Account/Category.
            builder.HasQueryFilter(r => !EF.Property<bool>(r, "IsDeleted")
                && !EF.Property<bool>(r.Account!, "IsDeleted")
                && !EF.Property<bool>(r.Category!, "IsDeleted"));
        }
    }
}
