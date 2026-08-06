using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Wallet.Sync.BudgetBakers.Host.Synchronization
{
    /// <summary>
    /// Runs a single entity sync step (invoke, check result, log outcome) so that callers such as
    /// <see cref="Program"/> don't need to duplicate that logic for each entity type.
    /// </summary>
    public class SyncStepRunner
    {
        private readonly ILogger<SyncStepRunner> _logger;

        public SyncStepRunner(ILogger<SyncStepRunner> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Executes <paramref name="syncAsync"/>, logging failure/success outcomes, and on success
        /// logs a description of each synced entity via <paramref name="describeEntity"/>.
        /// </summary>
        /// <returns><c>true</c> if the sync succeeded; otherwise <c>false</c>.</returns>
        public async Task<bool> RunAsync<TEntity>(
            string entityTypeName,
            Func<CancellationToken, Task<Result>> syncAsync,
            IEnumerable<TEntity> syncedEntities,
            Func<TEntity, string> describeEntity,
            CancellationToken cancellationToken = default)
        {
            Result result = await syncAsync(cancellationToken);

            if (result.IsFailed)
            {
                _logger.LogError("{EntityType} sync failed:", entityTypeName);
                foreach (IError error in result.Errors)
                    _logger.LogError(" - {ErrorMessage}", error.Message);

                return false;
            }

            _logger.LogInformation("{EntityType} sync completed successfully.", entityTypeName);
            foreach (TEntity entity in syncedEntities)
                _logger.LogInformation("{EntityDescription}", describeEntity(entity));

            return true;
        }
    }
}
