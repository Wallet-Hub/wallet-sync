using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Data.Entities
{
    public class Account : IEntity
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string CurrencyCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
