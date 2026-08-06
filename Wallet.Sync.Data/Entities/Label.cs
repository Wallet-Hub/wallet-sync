using System;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Data.Entities
{
    public class Label : IEntity
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
