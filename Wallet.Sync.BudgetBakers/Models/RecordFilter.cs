using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.BudgetBakers.Models
{
    public class RecordFilter : ExternalEntityFilter
    {
        public DateTime? MinimumDate { get; set; }
        public DateTime? MaximumDate { get; set; }
    }
}
