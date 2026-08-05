using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.BudgetBakers.Models
{
    public class AccountFilter : ExternalEntityFilter
    {
        public IList<string> Ids { get; set; } = [];
        public string? Type { get; set; }
    }
}
