using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataAccount = Wallet.Sync.Data.Entities.Account;

namespace Wallet.Sync.Data.EfCore.Repositories
{
    /// <summary>
    /// Persists <see cref="DataAccount"/> entities retrieved from the provider into the local database.
    /// </summary>
    public class AccountRepository
    {
        private readonly WalletSyncDbContext _dbContext;

        public AccountRepository(WalletSyncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Inserts the account if it doesn't exist locally (matched by <see cref="DataAccount.ExternalId"/>),
        /// otherwise updates its scalar properties. Soft-deleted accounts that reappear are revived, since
        /// the provider returning them means they still exist upstream.
        /// </summary>
        public async Task UpsertAsync(DataAccount account, CancellationToken cancellationToken = default)
        {
            DataAccount? existing = await _dbContext.Accounts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.ExternalId == account.ExternalId, cancellationToken);

            if (existing is null)
            {
                _dbContext.Accounts.Add(account);
                return;
            }

            existing.CurrencyCode = account.CurrencyCode;
            existing.Name = account.Name;
            existing.Type = account.Type;
            existing.Color = account.Color;
            existing.CreatedAt = account.CreatedAt;
            existing.UpdatedAt = account.UpdatedAt;

            _dbContext.Entry(existing).Property("IsDeleted").CurrentValue = false;
        }
    }
}
