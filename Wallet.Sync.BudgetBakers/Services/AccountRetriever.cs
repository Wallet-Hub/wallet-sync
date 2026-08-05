using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBakers.Wallet.Net.Models;
using BudgetBakers.Wallet.Net.Models.Account;
using BudgetBakers.Wallet.Net.Services.Clients;
using FluentResults;
using Microsoft.Extensions.Logging;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.Core.Services;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class AccountRetriever : IExternalEntityRetriever<Account, AccountFilter>
    {
        private readonly AccountClient _accountClient;
        private readonly PagedEntityFetcher<Account, AccountFilter> _pagedFetcher;

        public AccountRetriever(AccountClient accountClient, ILogger<AccountRetriever> logger)
        {
            _accountClient = accountClient;
            _pagedFetcher = new PagedEntityFetcher<Account, AccountFilter>(FetchPageAsync, logger);
        }

        public Task<Result<IList<Account>>> RetrieveAsync(AccountFilter filter) => _pagedFetcher.RetrieveAllAsync(filter);

        private async Task<Result<EntityPage<Account>>> FetchPageAsync(AccountFilter filter, int offset)
        {
            AccountType? accountType = filter.Type is null ? null : (AccountType)Enum.Parse(typeof(AccountType), filter.Type, true);

            GetAccountsRequest accountsRequest = new()
            {
                Ids = filter.Ids,
                AccountType = accountType,
                Offset = offset,
                Limit = filter.PageLength
            };

            if (filter.MinimumUpdatedAt.HasValue)
            {
                DateFilter dateFilter = new()
                {
                    Prefix = RangePrefix.GreaterThanOrEqual,
                    Value = filter.MinimumUpdatedAt.Value
                };

                accountsRequest.UpdatedAt = [dateFilter];
            }

            Result<GetAccountsResponse> response = await _accountClient.GetAsync(accountsRequest);

            if (response.IsFailed)
                return Result.Fail(response.Errors);

            return Result.Ok(new EntityPage<Account>(response.Value.Accounts, response.Value.Pagination));
        }
    }
}
