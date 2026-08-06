using BudgetBakers.Wallet.Net.Models.Category;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Services;
using DataCategory = Wallet.Sync.Data.Entities.Category;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class CategoryMapper : IEntityMapper<Category, DataCategory>
    {
        public DataCategory Map(Category source)
        {
            return new DataCategory()
            {
                ExternalId = source.Id!,
                Name = source.Name ?? string.Empty,
                GroupId = source.Group!.Id!,
                GroupName = source.Group!.Name!,
                Color = source.Color,
                ParentExternalId = source.ParentId,
                CreatedAt = source.CreatedAt ?? default,
                UpdatedAt = source.UpdatedAt ?? default
            };
        }
    }
}
