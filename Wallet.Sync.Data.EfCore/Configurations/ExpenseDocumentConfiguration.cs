using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore.Configurations
{
    public class ExpenseDocumentConfiguration : IEntityTypeConfiguration<ExpenseDocument>
    {
        public void Configure(EntityTypeBuilder<ExpenseDocument> builder)
        {
            builder.ToTable("ExpenseDocuments");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(2048)
                .HasConversion(
                    url => url.ToString(),
                    value => new Uri(value));

            builder.Property(e => e.Description)
                .HasMaxLength(1024);

            builder.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
