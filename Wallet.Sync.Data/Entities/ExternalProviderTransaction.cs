using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Data.Entities
{
    /// <summary>
    /// Manually tracked link between a <see cref="Record"/> and the external transaction/order that
    /// generated it on an external provider account (e.g. an Amazon order id). Not synchronized from any provider.
    /// </summary>
    public class ExternalProviderTransaction
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public Record? Record { get; set; }
        public int ExternalProviderAccountId { get; set; }
        public ExternalProviderAccount? ExternalProviderAccount { get; set; }
        public string TransactionId { get; set; } = null!;
        public string? Description { get; set; }
    }
}
