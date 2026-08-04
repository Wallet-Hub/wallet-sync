using System;
using System.Threading;
using System.Threading.Tasks;
using BudgetBakers.Wallet.Net.Services;
using Microsoft.Extensions.Options;

namespace Wallet.Sync.BudgetBakers.Host
{
    /// <summary>
    /// Reads the BudgetBakers access token from configuration (see <see cref="BudgetBakersOptions"/>).
    /// </summary>
    public class ConfigurationAccessTokenProvider : IAccessTokenProvider
    {
        private readonly BudgetBakersOptions _options;

        public ConfigurationAccessTokenProvider(IOptions<BudgetBakersOptions> options)
        {
            _options = options.Value;
        }

        public Task<string> GetAccessTokenAsync(CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_options.AccessToken))
                throw new InvalidOperationException($"Configuration section '{BudgetBakersOptions.SectionName}:AccessToken' is not set.");

            return Task.FromResult(_options.AccessToken);
        }
    }
}
