using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DataLabel = Wallet.Sync.Data.Entities.Label;
using DataRecord = Wallet.Sync.Data.Entities.Record;

namespace Wallet.Sync.Data.EfCore.Repositories
{
    /// <summary>
    /// Persists <see cref="DataRecord"/> entities retrieved from the provider into the local database.
    /// </summary>
    public class RecordRepository
    {
        private readonly WalletSyncDbContext _dbContext;
        private readonly LabelRepository _labelRepository;

        public RecordRepository(WalletSyncDbContext dbContext, LabelRepository labelRepository)
        {
            _dbContext = dbContext;
            _labelRepository = labelRepository;
        }

        /// <summary>
        /// Inserts the record if it doesn't exist locally (matched by <see cref="DataRecord.ExternalId"/>),
        /// otherwise updates its scalar properties. Soft-deleted records that reappear are revived, since
        /// the provider returning them means they still exist upstream.
        /// The owning Account/Category are resolved locally by their external id; if either one is not
        /// found (e.g. not synced yet), the record is skipped.
        /// Labels carried by the record are upserted via <see cref="LabelRepository"/> (matched by
        /// <see cref="DataLabel.ExternalId"/>) and attached to the record, since the provider embeds full
        /// label data on each record.
        /// </summary>
        public async Task<bool> UpsertAsync(DataRecord record, CancellationToken cancellationToken = default)
        {
            int? accountId = await _dbContext.Accounts
                .IgnoreQueryFilters()
                .Where(a => a.ExternalId == record.ExternalAccountId)
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync(cancellationToken);

            int? categoryId = await _dbContext.Categories
                .IgnoreQueryFilters()
                .Where(c => c.ExternalId == record.ExternalCategoryId)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (accountId is null || categoryId is null)
                return false;

            List<DataLabel> resolvedLabels = await _labelRepository.ResolveAsync(record.Labels, cancellationToken);

            DataRecord? existing = await _dbContext.Records
                .IgnoreQueryFilters()
                .Include(r => r.Labels)
                .FirstOrDefaultAsync(r => r.ExternalId == record.ExternalId, cancellationToken);

            if (existing is null)
            {
                record.AccountId = accountId.Value;
                record.CategoryId = categoryId.Value;
                record.Labels = resolvedLabels;
                _dbContext.Records.Add(record);
                return true;
            }

            existing.AccountId = accountId.Value;
            existing.ExternalAccountId = record.ExternalAccountId;
            existing.CategoryId = categoryId.Value;
            existing.ExternalCategoryId = record.ExternalCategoryId;
            existing.Description = record.Description;
            existing.RecordDate = record.RecordDate;
            existing.Amount = record.Amount;
            existing.RecordState = record.RecordState;
            existing.RecordType = record.RecordType;
            existing.CreatedAt = record.CreatedAt;
            existing.UpdatedAt = record.UpdatedAt;

            existing.Labels.Clear();
            foreach (DataLabel label in resolvedLabels)
                existing.Labels.Add(label);

            _dbContext.Entry(existing).Property("IsDeleted").CurrentValue = false;

            return true;
        }
    }
}
