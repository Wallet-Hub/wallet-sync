using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataCategory = Wallet.Sync.Data.Entities.Category;

namespace Wallet.Sync.Data.EfCore.Repositories
{
    /// <summary>
    /// Persists <see cref="DataCategory"/> entities retrieved from the provider into the local database.
    /// </summary>
    public class CategoryRepository
    {
        private readonly WalletSyncDbContext _dbContext;

        public CategoryRepository(WalletSyncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Inserts the category if it doesn't exist locally (matched by <see cref="DataCategory.ExternalId"/>),
        /// otherwise updates its scalar properties. Soft-deleted categories that reappear are revived, since
        /// the provider returning them means they still exist upstream.
        /// </summary>
        public async Task UpsertAsync(DataCategory category, CancellationToken cancellationToken = default)
        {
            DataCategory? existing = await _dbContext.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ExternalId == category.ExternalId, cancellationToken);

            if (existing is null)
            {
                _dbContext.Categories.Add(category);
                return;
            }

            existing.Name = category.Name;
            existing.GroupId = category.GroupId;
            existing.GroupName = category.GroupName;
            existing.Color = category.Color;
            existing.CreatedAt = category.CreatedAt;
            existing.UpdatedAt = category.UpdatedAt;

            _dbContext.Entry(existing).Property("IsDeleted").CurrentValue = false;
        }
    }
}
