using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Data.Entities
{
    public class ExpenseDocument
    {
        public Uri Url { get; set; } = null!;
        public string? Description { get; set; }
    }
}
