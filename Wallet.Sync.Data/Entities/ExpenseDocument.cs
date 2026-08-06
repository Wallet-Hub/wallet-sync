using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Data.Entities
{
    public class ExpenseDocument
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public Record? Record { get; set; }
        public int ProviderId { get; set; }
        public ExternalProvider? Provider { get; set; }
        public Uri Url { get; set; } = null!;
        public string? Description { get; set; }
    }
}
