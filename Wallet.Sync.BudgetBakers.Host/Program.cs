using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBakers.Wallet.Net.Models.Account;
using BudgetBakers.Wallet.Net.Services;
using BudgetBakers.Wallet.Net.Services.Clients;
using BudgetBakers.Wallet.Net.Utility;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wallet.Sync.BudgetBakers.Services;
using Wallet.Sync.Core.Services;
using DataAccount = Wallet.Sync.Data.Entities.Account;

namespace Wallet.Sync.BudgetBakers.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

            builder.Services.Configure<BudgetBakersOptions>(builder.Configuration.GetSection(BudgetBakersOptions.SectionName));
            builder.Services.Configure<AccountFilter>(builder.Configuration.GetSection("AccountFilter"));

            builder.Services.AddSingleton<IAccessTokenProvider, ConfigurationAccessTokenProvider>();
            builder.Services.AddWalletClient<AccountClient>();

            builder.Services.AddTransient<AccountRetriever>();
            builder.Services.AddTransient<IExternalEntityRetriever<Account, AccountFilter>>(
                serviceProvider => serviceProvider.GetRequiredService<AccountRetriever>());

            builder.Services.AddSingleton<IEntityMapper<Account, DataAccount>, AccountMapper>();
            builder.Services.AddTransient<MappingEntityRetriever<Account, AccountFilter, DataAccount>>();

            using IHost host = builder.Build();

            MappingEntityRetriever<Account, AccountFilter, DataAccount> accountEntityRetriever =
                host.Services.GetRequiredService<MappingEntityRetriever<Account, AccountFilter, DataAccount>>();
            AccountFilter filter = host.Services.GetRequiredService<IOptions<AccountFilter>>().Value;

            Result<IList<DataAccount>> result = await accountEntityRetriever.RetrieveAsync(filter);

            if (result.IsFailed)
            {
                Console.WriteLine("Failed to retrieve accounts:");
                foreach (IError error in result.Errors)
                    Console.WriteLine($" - {error.Message}");

                return;
            }

            Console.WriteLine($"Retrieved {result.Value.Count} account(s):");
            foreach (DataAccount account in result.Value)
                Console.WriteLine($" - {account.ExternalId} | {account.Name} ({account.CurrencyCode})");
        }
    }
}

