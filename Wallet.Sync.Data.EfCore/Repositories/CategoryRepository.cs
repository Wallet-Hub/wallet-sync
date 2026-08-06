using System.Collections.Generic;
using System.Linq;
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
        /// Note: <see cref="DataCategory.ParentId"/> is NOT resolved here, since the parent category may not
        /// have been upserted yet within the same sync batch. Call <see cref="ResolveParentsAsync"/> once all
        /// categories in the batch have been upserted and saved.
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
            existing.ParentExternalId = category.ParentExternalId;
            existing.CreatedAt = category.CreatedAt;
            existing.UpdatedAt = category.UpdatedAt;

            _dbContext.Entry(existing).Property("IsDeleted").CurrentValue = false;
        }

        /// <summary>
        /// Resolves <see cref="DataCategory.ParentId"/> from <see cref="DataCategory.ParentExternalId"/> for every
        /// locally stored category. Must be called after all categories in the current sync batch have been
        /// upserted and saved, so that every potential parent already has an assigned local <see cref="DataCategory.Id"/>.
        /// Categories whose parent external id cannot be resolved locally (e.g. not synced yet) are left untouched.
        /// </summary>
        public async Task ResolveParentsAsync(CancellationToken cancellationToken = default)
        {
            List<DataCategory> categories = await _dbContext.Categories
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            Dictionary<string, int> idByExternalId = categories.ToDictionary(c => c.ExternalId, c => c.Id);

            foreach (DataCategory category in categories)
            {
                if (category.ParentExternalId is null)
                {
                    category.ParentId = null;
                    continue;
                }

                if (idByExternalId.TryGetValue(category.ParentExternalId, out int parentId))
                    category.ParentId = parentId;
            }
        }
    }
}
