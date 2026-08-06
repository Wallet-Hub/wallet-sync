using Microsoft.EntityFrameworkCore;
using Wallet.Sync.Data.EfCore.Entities;
using Wallet.Sync.Data.Entities;

namespace Wallet.Sync.Data.EfCore
{
    /// <summary>
    /// EF Core context for persisting Wallet.Sync.Data entities.
    /// Intentionally provider-agnostic: the concrete database provider (SQL Server, SQLite, Npgsql, ...)
    /// must be configured by the consuming host via the <see cref="DbContextOptions"/> passed in.
    /// </summary>
    public class WalletSyncDbContext : DbContext
    {
        public WalletSyncDbContext(DbContextOptions<WalletSyncDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Record> Records => Set<Record>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<ExpenseDocument> ExpenseDocuments => Set<ExpenseDocument>();
        public DbSet<ExternalProvider> ExternalProviders => Set<ExternalProvider>();
        public DbSet<ExternalProviderAccount> ExternalProviderAccounts => Set<ExternalProviderAccount>();
        public DbSet<ExternalProviderTransaction> ExternalProviderTransactions => Set<ExternalProviderTransaction>();
        public DbSet<SyncState> SyncStates => Set<SyncState>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(WalletSyncDbContext).Assembly);
        }
    }
}
