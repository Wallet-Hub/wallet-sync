using System.Threading.Tasks;
using BudgetBakers.Wallet.Net.Models.Account;
using BudgetBakers.Wallet.Net.Models.Category;
using BudgetBakers.Wallet.Net.Models.Record;
using BudgetBakers.Wallet.Net.Services;
using BudgetBakers.Wallet.Net.Services.Clients;
using BudgetBakers.Wallet.Net.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Wallet.Sync.BudgetBakers.Host.Configuration;
using Wallet.Sync.BudgetBakers.Host.Extensions;
using Wallet.Sync.BudgetBakers.Host.Synchronization;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.BudgetBakers.Services;
using Wallet.Sync.Core.Services;
using Wallet.Sync.Data.EfCore;
using Wallet.Sync.Data.EfCore.Repositories;
using DataAccount = Wallet.Sync.Data.Entities.Account;
using DataCategory = Wallet.Sync.Data.Entities.Category;
using DataRecord = Wallet.Sync.Data.Entities.Record;

namespace Wallet.Sync.BudgetBakers.Host
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.Configuration.AddUserSecrets<Program>();

            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            builder.Services.Configure<BudgetBakersOptions>(builder.Configuration.GetSection(BudgetBakersOptions.SectionName));
            builder.Services.Configure<AccountFilter>(builder.Configuration.GetSection(nameof(AccountFilter)));
            builder.Services.Configure<CategoryFilter>(builder.Configuration.GetSection(nameof(CategoryFilter)));
            builder.Services.Configure<RecordFilter>(builder.Configuration.GetSection(nameof(RecordFilter)));

            builder.AddFileSystemSecrets();

            builder.Services.AddSingleton<IAccessTokenProvider, ConfigurationAccessTokenProvider>();
            builder.Services.AddWalletClient<AccountClient>();
            builder.Services.AddWalletClient<CategoryClient>();
            builder.Services.AddWalletClient<RecordClient>();

            builder.Services.AddTransient<AccountRetriever>();
            builder.Services.AddTransient<IExternalEntityRetriever<Account, AccountFilter>>(
                serviceProvider => serviceProvider.GetRequiredService<AccountRetriever>());

            builder.Services.AddTransient<CategoryRetriever>();
            builder.Services.AddTransient<IExternalEntityRetriever<Category, CategoryFilter>>(
                serviceProvider => serviceProvider.GetRequiredService<CategoryRetriever>());

            builder.Services.AddTransient<RecordRetriever>();
            builder.Services.AddTransient<IExternalEntityRetriever<Record, RecordFilter>>(
                serviceProvider => serviceProvider.GetRequiredService<RecordRetriever>());

            builder.Services.AddSingleton<IEntityMapper<Account, DataAccount>, AccountMapper>();
            builder.Services.AddTransient<MappingEntityRetriever<Account, AccountFilter, DataAccount>>();

            builder.Services.AddSingleton<IEntityMapper<Category, DataCategory>, CategoryMapper>();
            builder.Services.AddTransient<MappingEntityRetriever<Category, CategoryFilter, DataCategory>>();

            builder.Services.AddSingleton<IEntityMapper<Record, DataRecord>, RecordMapper>();
            builder.Services.AddTransient<MappingEntityRetriever<Record, RecordFilter, DataRecord>>();

            builder.Services.AddDbContext<WalletSyncDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("WalletSync")));

            builder.Services.AddScoped<AccountRepository>();
            builder.Services.AddScoped<AccountSyncService>();

            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<CategorySyncService>();

            builder.Services.AddScoped<LabelRepository>();
            builder.Services.AddScoped<RecordRepository>();
            builder.Services.AddScoped<RecordSyncService>();

            builder.Services.AddScoped<SyncStepRunner>();

            using IHost host = builder.Build();

            using (IServiceScope scope = host.Services.CreateScope())
            {
                SyncStepRunner syncStepRunner = scope.ServiceProvider.GetRequiredService<SyncStepRunner>();

                WalletSyncDbContext dbContext = scope.ServiceProvider.GetRequiredService<WalletSyncDbContext>();
                await dbContext.Database.EnsureCreatedAsync();

                AccountSyncService accountSyncService = scope.ServiceProvider.GetRequiredService<AccountSyncService>();
                bool accountSyncSucceeded = await syncStepRunner.RunAsync(
                    "Account",
                    accountSyncService.SyncAsync,
                    dbContext.Accounts,
                    account => $" - {account.ExternalId} | {account.Name} ({account.CurrencyCode})");

                if (!accountSyncSucceeded)
                    return;

                CategorySyncService categorySyncService = scope.ServiceProvider.GetRequiredService<CategorySyncService>();
                bool categorySyncSucceeded = await syncStepRunner.RunAsync(
                    "Category",
                    categorySyncService.SyncAsync,
                    dbContext.Categories,
                    category => $" - {category.ExternalId} | {category.Name} ({category.GroupName})");

                if (!categorySyncSucceeded)
                    return;

                RecordSyncService recordSyncService = scope.ServiceProvider.GetRequiredService<RecordSyncService>();
                bool recordSyncSucceeded = await syncStepRunner.RunAsync(
                    "Record",
                    recordSyncService.SyncAsync,
                    dbContext.Records,
                    record => $" - {record.ExternalId} | {record.Description} ({record.Amount})");

                if (!recordSyncSucceeded)
                    return;
            }
        }
    }
}

