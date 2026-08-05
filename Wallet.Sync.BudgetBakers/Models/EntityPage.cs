using System.Collections.Generic;
using BudgetBakers.Wallet.Net.Models.Pagination;

namespace Wallet.Sync.BudgetBakers.Models
{
    /// <summary>
    /// Represents a single page of entities returned by a BudgetBakers API endpoint,
    /// together with the pagination metadata needed to fetch subsequent pages.
    /// </summary>
    public class EntityPage<TEntity>
        where TEntity : class
    {
        public IList<TEntity> Items { get; }
        public PaginationInfo Pagination { get; }

        public EntityPage(IList<TEntity> items, PaginationInfo pagination)
        {
            Items = items;
            Pagination = pagination;
        }
    }
}
