using BudgetBakers.Wallet.Net.Models;
using BudgetBakers.Wallet.Net.Models.Record;
using BudgetBakers.Wallet.Net.Services.Clients;
using FluentResults;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.Core.Services;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class RecordRetriever : IExternalEntityRetriever<Record, RecordFilter>
    {
        private readonly RecordClient _recordClient;
        private readonly PagedEntityFetcher<Record, RecordFilter> _pagedFetcher;

        public RecordRetriever(RecordClient recordClient, ILogger<RecordRetriever> logger)
        {
            _recordClient = recordClient;
            _pagedFetcher = new PagedEntityFetcher<Record, RecordFilter>(FetchPageAsync, logger);
        }

        public Task<Result<IList<Record>>> RetrieveAsync(RecordFilter filter) => _pagedFetcher.RetrieveAllAsync(filter);

        private async Task<Result<EntityPage<Record>>> FetchPageAsync(RecordFilter filter, int offset)
        {
            GetRecordsRequest recordsRequest = new()
            {
                Offset = offset,
                Limit = filter.PageLength,
                ConvertTo = "EUR"
            };

            if (filter.MinimumDate.HasValue)
            {
                DateFilter dateFilter = new()
                {
                    Prefix = RangePrefix.GreaterThanOrEqual,
                    Value = filter.MinimumDate.Value
                };

                recordsRequest.RecordDate = [dateFilter];
            }

            if (filter.MinimumUpdatedAt.HasValue)
            {
                DateFilter dateFilter = new()
                {
                    Prefix = RangePrefix.GreaterThanOrEqual,
                    Value = filter.MinimumUpdatedAt.Value
                };

                recordsRequest.UpdatedAt = [dateFilter];
            }

            Result<GetRecordsResponse> response = await _recordClient.GetAsync(recordsRequest);

            if (response.IsFailed)
                return Result.Fail(response.Errors);

            return Result.Ok(new EntityPage<Record>(response.Value.Records, response.Value.Pagination));
        }
    }
}
