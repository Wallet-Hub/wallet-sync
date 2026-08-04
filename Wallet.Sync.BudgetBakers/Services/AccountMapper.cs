using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Services;
using BudgetBakers.Wallet.Net.Models.Account;
using DataAccount = Wallet.Sync.Data.Entities.Account;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class AccountMapper : IEntityMapper<Account, DataAccount>
    {
        public DataAccount Map(Account source)
        {
            return new DataAccount
            {
                ExternalId = source.Id ?? string.Empty,
                CurrencyCode = source.CurrencyCode ?? string.Empty,
                Name = source.Name ?? string.Empty,
                Type = source.AccountType?.ToString() ?? string.Empty,
                CreatedAt = source.CreatedAt ?? default,
                UpdatedAt = source.UpdatedAt ?? default
            };
        }
    }
}
