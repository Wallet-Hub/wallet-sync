using BudgetBakers.Wallet.Net.Models.Record;
using Wallet.Sync.Core.Services;
using DataRecord = Wallet.Sync.Data.Entities.Record;
using DataRecordState = Wallet.Sync.Data.Entities.RecordState;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class RecordMapper : IEntityMapper<Record, DataRecord>
    {
        public DataRecord Map(Record source)
        {
            return new DataRecord
            {
                ExternalId = source.Id!,
                ExternalAccountId = source.AccountId!,
                Description = source.Note ?? string.Empty,
                RecordDate = source.RecordDate ?? default,
                ExternalCategoryId = source.Category?.Id ?? string.Empty,
                Amount = source.ConvertedAmount!.Value!.Value,
                RecordState = source.RecordState == RecordState.Reconciled ? DataRecordState.Verified : DataRecordState.NotVerified,
                RecordType = source.RecordType?.ToString() ?? string.Empty,
                CreatedAt = source.CreatedAt ?? default,
                UpdatedAt = source.UpdatedAt ?? default
            };
        }
    }
}
