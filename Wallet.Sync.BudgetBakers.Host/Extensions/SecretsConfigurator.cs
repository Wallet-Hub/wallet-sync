using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.BudgetBakers.Host.Configuration;

namespace Wallet.Sync.BudgetBakers.Host.Extensions
{
    public static class SecretsConfigurator
    {
        public static void AddFileSystemSecrets(this IHostApplicationBuilder builder)
        {
            string? secretsPath = builder.Configuration["FileSystemSecrets:Path"];

            if (string.IsNullOrEmpty(secretsPath))
                return;

            builder.Configuration.AddJsonFile(path: secretsPath, optional: false, reloadOnChange: false);
        }
    }
}
