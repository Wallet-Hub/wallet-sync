using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.Core.Services;
using Wallet.Sync.Data.EfCore;
using Wallet.Sync.Data.EfCore.Entities;
using Wallet.Sync.Data.EfCore.Repositories;
using Record = BudgetBakers.Wallet.Net.Models.Record.Record;
using DataRecord = Wallet.Sync.Data.Entities.Record;
using DataRecordState = Wallet.Sync.Data.Entities.RecordState;

namespace Wallet.Sync.BudgetBakers.Host.Synchronization
{
    /// <summary>
    /// Synchronizes BudgetBakers records into the local database using delta synchronization:
    /// only records updated since the last successful sync (tracked via <see cref="SyncState"/>)
    /// are requested from the provider. Only records whose <see cref="DataRecord.RecordState"/>
    /// is <see cref="DataRecordState.Verified"/> are persisted locally.
    /// </summary>
    public class RecordSyncService
    {
        private const string EntityTypeName = "Record";

        private readonly MappingEntityRetriever<Record, RecordFilter, DataRecord> _recordRetriever;
        private readonly RecordRepository _recordRepository;
        private readonly WalletSyncDbContext _dbContext;
        private readonly RecordFilter _configuredFilter;
        private readonly ILogger<RecordSyncService> _logger;

        public RecordSyncService(
            MappingEntityRetriever<Record, RecordFilter, DataRecord> recordRetriever,
            RecordRepository recordRepository,
            WalletSyncDbContext dbContext,
            IOptions<RecordFilter> configuredFilter,
            ILogger<RecordSyncService> logger)
        {
            _recordRetriever = recordRetriever;
            _recordRepository = recordRepository;
            _dbContext = dbContext;
            _configuredFilter = configuredFilter.Value;
            _logger = logger;
        }

        public async Task<Result> SyncAsync(CancellationToken cancellationToken = default)
        {
            SyncState? syncState = await _dbContext.SyncStates
                .SingleOrDefaultAsync(s => s.EntityType == EntityTypeName, cancellationToken);

            bool isNewSyncState = syncState is null;
            syncState ??= new SyncState { EntityType = EntityTypeName };

            DateTime syncStartedAt = DateTime.UtcNow;

            // MinimumDate/MaximumDate/PageLength come from configuration (appsettings); MinimumUpdatedAt
            // comes exclusively from the persisted sync state, never from configuration.
            RecordFilter filter = new()
            {
                PageLength = _configuredFilter.PageLength,
                MinimumDate = _configuredFilter.MinimumDate,
                MaximumDate = _configuredFilter.MaximumDate,
                MinimumUpdatedAt = syncState.LastSyncedAt
            };

            _logger.LogInformation(
                "Starting Record delta sync (MinimumUpdatedAt: {MinimumUpdatedAt})",
                filter.MinimumUpdatedAt is null ? "none (full sync)" : filter.MinimumUpdatedAt.Value.ToString("O"));

            Result<IList<DataRecord>> result = await _recordRetriever.RetrieveAsync(filter);

            if (result.IsFailed)
            {
                string errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger.LogError("Record sync failed: {Error}", errorMessage);

                syncState.Status = SyncStatus.Failed;
                syncState.LastError = errorMessage;
                syncState.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                int persistedCount = 0;
                int skippedCount = 0;

                foreach (DataRecord record in result.Value)
                {
                    // Only verified records are stored locally.
                    if (record.RecordState != DataRecordState.Verified)
                    {
                        skippedCount++;
                        continue;
                    }

                    bool persisted = await _recordRepository.UpsertAsync(record, cancellationToken);
                    if (persisted)
                        persistedCount++;
                    else
                        skippedCount++;
                }

                syncState.LastSyncedAt = syncStartedAt;
                syncState.Status = SyncStatus.Succeeded;
                syncState.LastError = null;
                syncState.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Record delta sync: {Persisted} record(s) persisted, {Skipped} record(s) skipped",
                    persistedCount, skippedCount);
            }

            if (isNewSyncState)
                _dbContext.SyncStates.Add(syncState);

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (result.IsFailed)
                return Result.Fail(result.Errors);

            _logger.LogInformation("Record delta sync completed, {Count} record(s) retrieved", result.Value.Count);

            return Result.Ok();
        }
    }
}
