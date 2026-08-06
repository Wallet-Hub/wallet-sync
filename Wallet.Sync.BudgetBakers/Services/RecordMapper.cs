using System.Linq;
using BudgetBakers.Wallet.Net.Models.Record;
using Wallet.Sync.Core.Services;
using DataLabel = Wallet.Sync.Data.Entities.Label;
using DataRecord = Wallet.Sync.Data.Entities.Record;
using DataRecordState = Wallet.Sync.Data.Entities.RecordState;
using ProviderLabel = BudgetBakers.Wallet.Net.Models.Label.Label;

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
                UpdatedAt = source.UpdatedAt ?? default,
                Labels = source.Labels?.Select(MapLabel).ToList() ?? []
            };
        }

        private static DataLabel MapLabel(ProviderLabel source)
        {
            return new DataLabel
            {
                ExternalId = source.Id!,
                Name = source.Name ?? string.Empty,
                Color = source.Color,
                CreatedAt = source.CreatedAt ?? default,
                UpdatedAt = source.UpdatedAt ?? default
            };
        }
    }
}
