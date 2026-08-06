using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Data.Entities
{
    public class ExternalProviderAccount
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public ExternalProvider? Provider { get; set; }
        public string Reference { get; set; } = null!;
        public string? Description { get; set; }
    }
}
