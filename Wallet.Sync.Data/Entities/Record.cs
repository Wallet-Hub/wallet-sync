using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Data.Entities
{
    public class Record : IEntity
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public int AccountId { get; set; }
        public string ExternalAccountId { get; set; } = null!;
        public Account? Account { get; set; }
        public string Description { get; set; } = null!;
        public DateTime RecordDate { get; set; }
        public int CategoryId { get; set; }
        public string ExternalCategoryId { get; set; } = null!;
        public Category? Category { get; set; }
        public decimal Amount { get; set; }
        public RecordState RecordState { get; set; }
        public string RecordType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IList<ExpenseDocument> ExpenseDocuments { get; set; } = [];
        public IList<Label> Labels { get; set; } = [];
        public string? Note { get; set; }
    }
}
