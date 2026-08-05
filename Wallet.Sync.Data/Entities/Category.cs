using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Data.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string GroupId { get; set; } = null!;
        public string GroupName { get; set; } = null!;
        public string? Color { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
