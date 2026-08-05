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
using Account = BudgetBakers.Wallet.Net.Models.Account.Account;
using DataAccount = Wallet.Sync.Data.Entities.Account;

namespace Wallet.Sync.BudgetBakers.Host.Synchronization
{
    /// <summary>
    /// Synchronizes BudgetBakers accounts into the local database using delta synchronization:
    /// only accounts updated since the last successful sync (tracked via <see cref="SyncState"/>)
    /// are requested from the provider.
    /// </summary>
    public class AccountSyncService
    {
        private const string EntityTypeName = "Account";

        private readonly MappingEntityRetriever<Account, AccountFilter, DataAccount> _accountRetriever;
        private readonly AccountRepository _accountRepository;
        private readonly WalletSyncDbContext _dbContext;
        private readonly AccountFilter _configuredFilter;
        private readonly ILogger<AccountSyncService> _logger;

        public AccountSyncService(
            MappingEntityRetriever<Account, AccountFilter, DataAccount> accountRetriever,
            AccountRepository accountRepository,
            WalletSyncDbContext dbContext,
            IOptions<AccountFilter> configuredFilter,
            ILogger<AccountSyncService> logger)
        {
            _accountRetriever = accountRetriever;
            _accountRepository = accountRepository;
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

            // Ids/Type/PageLength come from configuration (appsettings); MinimumUpdatedAt
            // comes exclusively from the persisted sync state, never from configuration.
            AccountFilter filter = new()
            {
                PageLength = _configuredFilter.PageLength,
                Ids = _configuredFilter.Ids,
                Type = _configuredFilter.Type,
                MinimumUpdatedAt = syncState.LastSyncedAt
            };

            _logger.LogInformation(
                "Starting Account delta sync (MinimumUpdatedAt: {MinimumUpdatedAt})",
                filter.MinimumUpdatedAt is null ? "none (full sync)" : filter.MinimumUpdatedAt.Value.ToString("O"));

            Result<IList<DataAccount>> result = await _accountRetriever.RetrieveAsync(filter);

            if (result.IsFailed)
            {
                string errorMessage = string.Join("; ", result.Errors.Select(e => e.Message));
                _logger.LogError("Account sync failed: {Error}", errorMessage);

                syncState.Status = SyncStatus.Failed;
                syncState.LastError = errorMessage;
                syncState.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                foreach (DataAccount account in result.Value)
                    await _accountRepository.UpsertAsync(account, cancellationToken);

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

            _logger.LogInformation("Account delta sync completed, {Count} account(s) processed", result.Value.Count);

            return Result.Ok();
        }
    }
}
