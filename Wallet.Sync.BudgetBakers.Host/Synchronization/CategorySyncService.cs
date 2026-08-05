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
using Category = BudgetBakers.Wallet.Net.Models.Category.Category;
using DataCategory = Wallet.Sync.Data.Entities.Category;

namespace Wallet.Sync.BudgetBakers.Host.Synchronization
{
    /// <summary>
    /// Synchronizes BudgetBakers categories into the local database using delta synchronization:
    /// only categories updated since the last successful sync (tracked via <see cref="SyncState"/>)
    /// are requested from the provider.
    /// </summary>
    public class CategorySyncService
    {
        private const string EntityTypeName = "Category";

        private readonly MappingEntityRetriever<Category, CategoryFilter, DataCategory> _categoryRetriever;
        private readonly CategoryRepository _categoryRepository;
        private readonly WalletSyncDbContext _dbContext;
        private readonly CategoryFilter _configuredFilter;
        private readonly ILogger<CategorySyncService> _logger;

        public CategorySyncService(
            MappingEntityRetriever<Category, CategoryFilter, DataCategory> categoryRetriever,
            CategoryRepository categoryRepository,
            WalletSyncDbContext dbContext,
            IOptions<CategoryFilter> configuredFilter,
            ILogger<CategorySyncService> logger)
        {
            _categoryRetriever = categoryRetriever;
            _categoryRepository = categoryRepository;
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

            // Other filters come from configuration (appsettings); MinimumUpdatedAt
            // comes exclusively from the persisted sync state, never from configuration.
            CategoryFilter filter = new()
            {
                PageLength = _configuredFilter.PageLength,
                MinimumUpdatedAt = syncState.LastSyncedAt
            };

            _logger.LogInformation(
                "Starting Category delta sync (MinimumUpdatedAt: {MinimumUpdatedAt})",
                filter.MinimumUpdatedAt is null ? "none (full sync)" : filter.MinimumUpdatedAt.Value.ToString("O"));

            Result<IList<DataCategory>> result = await _categoryRetriever.RetrieveAsync(filter);

            if (result.IsFailed)
            {
                string errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger.LogError("Category sync failed: {Error}", errorMessage);

                syncState.Status = SyncStatus.Failed;
                syncState.LastError = errorMessage;
                syncState.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                foreach (DataCategory category in result.Value)
                    await _categoryRepository.UpsertAsync(category, cancellationToken);

                syncState.LastSyncedAt = syncStartedAt;
                syncState.Status = SyncStatus.Succeeded;
                syncState.LastError = null;
                syncState.UpdatedAt = DateTime.UtcNow;
            }

            if (isNewSyncState)
                _dbContext.SyncStates.Add(syncState);

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (result.IsFailed)
                return Result.Fail(result.Errors);

            _logger.LogInformation("Category delta sync completed, {Count} category(ies) processed", result.Value.Count);

            return Result.Ok();
        }
    }
}
