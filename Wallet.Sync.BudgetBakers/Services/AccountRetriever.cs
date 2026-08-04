using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Services;
using BudgetBakers.Wallet.Net.Models;
using BudgetBakers.Wallet.Net.Services.Clients;
using BudgetBakers.Wallet.Net.Models.Account;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class AccountRetriever : IExternalEntityRetriever<Account, AccountFilter>
    {
        private readonly AccountClient _accountClient;
        private readonly ILogger<AccountRetriever> _logger;

        public AccountRetriever(AccountClient client, ILogger<AccountRetriever> logger)
        {
            _accountClient = client;
            _logger = logger;
        }

        public async Task<Result<IList<Account>>> RetrieveAsync(AccountFilter filter)
        {
            List<Account> retrievedAccounts = [];
            int offset = 0;
            bool hasMorePages;

            do
            {
                AccountType? accountType = filter.Type is null ? null : (AccountType)Enum.Parse(typeof(AccountType), filter.Type, true);

                _logger.LogDebug("Retrieving accounts with offset: {Offset}, limit: {Limit}", offset, filter.PageLength);

                GetAccountsRequest accountsRequest = new()
                {
                    Ids = filter.Ids,
                    AccountType = accountType,
                    Offset = offset,
                    Limit = filter.PageLength
                };

                Result<GetAccountsResponse> response = await _accountClient.GetAsync(accountsRequest);

                if (response.IsFailed)
                    return Result.Fail(response.Errors);

                GetAccountsResponse currentPage = response.Value;
                retrievedAccounts.AddRange(currentPage.Accounts);

                hasMorePages = currentPage.Accounts.Count == filter.PageLength
                    && (currentPage.Pagination.Total is null || retrievedAccounts.Count < currentPage.Pagination.Total.Value);

                offset = currentPage.Pagination.NextOffset;
            }
            while (hasMorePages);

            return Result.Ok<IList<Account>>(retrievedAccounts);
        }
    }
}
