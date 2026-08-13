using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataLabel = Wallet.Sync.Data.Entities.Label;

namespace Wallet.Sync.Data.EfCore.Repositories
{
    /// <summary>
    /// Persists <see cref="DataLabel"/> entities into the local database.
    /// </summary>
    public class LabelRepository
    {
        private readonly WalletSyncDbContext _dbContext;

        public LabelRepository(WalletSyncDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Upserts each label (matched by <see cref="DataLabel.ExternalId"/>), reviving it from a soft-deleted
        /// state if needed, and returns the corresponding tracked <see cref="DataLabel"/> instances so they can
        /// be attached to a record's Labels collection.
        /// </summary>
        public async Task<List<DataLabel>> ResolveAsync(IEnumerable<DataLabel> labels, CancellationToken cancellationToken = default)
        {
            List<DataLabel> sourceLabels = labels.ToList();
            if (sourceLabels.Count == 0)
                return [];

            List<string> externalIds = sourceLabels.Select(l => l.ExternalId)
                .Distinct()
                .ToList();

            //Retrieve all existing labels from the database, including soft-deleted ones, so we can update them if needed
            Dictionary<string, DataLabel> existingByExternalId = await _dbContext.Labels
                .IgnoreQueryFilters()
                .Where(l => externalIds.Contains(l.ExternalId))
                .ToDictionaryAsync(l => l.ExternalId, cancellationToken);

            //Also consider labels already tracked in this unit of work (e.g. added earlier in the same batch but
            //not yet saved), so we don't create duplicate entities that would violate the ExternalId unique constraint.
            foreach (DataLabel trackedLabel in _dbContext.ChangeTracker.Entries<DataLabel>()
                         .Where(e => e.State != EntityState.Detached && e.State != EntityState.Deleted)
                         .Select(e => e.Entity))
            {
                if (externalIds.Contains(trackedLabel.ExternalId))
                    existingByExternalId[trackedLabel.ExternalId] = trackedLabel;
            }

            List<DataLabel> resolved = [];

            foreach (DataLabel sourceLabel in sourceLabels)
            {
                //Label is already in the database, update it and revive it if it was soft-deleted
                if (existingByExternalId.TryGetValue(sourceLabel.ExternalId, out DataLabel? existingLabel))
                {
                    existingLabel.Name = sourceLabel.Name;
                    existingLabel.Color = sourceLabel.Color;
                    existingLabel.CreatedAt = sourceLabel.CreatedAt;
                    existingLabel.UpdatedAt = sourceLabel.UpdatedAt;

                    _dbContext.Entry(existingLabel).Property("IsDeleted").CurrentValue = false;

                    resolved.Add(existingLabel);
                }
                //Label is not in the database, add it
                else
                {
                    _dbContext.Labels.Add(sourceLabel);
                    existingByExternalId[sourceLabel.ExternalId] = sourceLabel;
                    resolved.Add(sourceLabel);
                }
            }

            return resolved;
        }
    }
}
