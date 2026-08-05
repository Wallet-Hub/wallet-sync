using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.BudgetBakers.Services
{
    /// <summary>
    /// Encapsulates the offset-based pagination loop shared by BudgetBakers entity retrievers.
    /// </summary>
    public class PagedEntityFetcher<TEntity, TFilter>
        where TEntity : class
        where TFilter : ExternalEntityFilter
    {
        private readonly Func<TFilter, int, Task<Result<EntityPage<TEntity>>>> _fetchPageAsync;
        private readonly ILogger _logger;

        public PagedEntityFetcher(
            Func<TFilter, int, Task<Result<EntityPage<TEntity>>>> fetchPageAsync,
            ILogger logger)
        {
            _fetchPageAsync = fetchPageAsync;
            _logger = logger;
        }

        public async Task<Result<IList<TEntity>>> RetrieveAllAsync(TFilter filter)
        {
            List<TEntity> retrievedEntities = [];
            int offset = 0;
            bool hasMorePages;

            do
            {
                _logger.LogDebug("Retrieving {EntityType} with offset: {Offset}, limit: {Limit}", typeof(TEntity).Name, offset, filter.PageLength);

                Result<EntityPage<TEntity>> pageResult = await _fetchPageAsync(filter, offset);

                if (pageResult.IsFailed)
                    return Result.Fail(pageResult.Errors);

                EntityPage<TEntity> page = pageResult.Value;
                retrievedEntities.AddRange(page.Items);

                hasMorePages = page.Items.Count == filter.PageLength
                    && (page.Pagination.Total is null || retrievedEntities.Count < page.Pagination.Total.Value);

                offset = page.Pagination.NextOffset;
            }
            while (hasMorePages);

            return Result.Ok<IList<TEntity>>(retrievedEntities);
        }
    }
}
