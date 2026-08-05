using System.Collections.Generic;
using System.Threading.Tasks;
using BudgetBakers.Wallet.Net.Models.Category;
using BudgetBakers.Wallet.Net.Services.Clients;
using FluentResults;
using Microsoft.Extensions.Logging;
using Wallet.Sync.BudgetBakers.Models;
using Wallet.Sync.Core.Services;

namespace Wallet.Sync.BudgetBakers.Services
{
    public class CategoryRetriever : IExternalEntityRetriever<Category, CategoryFilter>
    {
        private readonly CategoryClient _categoryClient;
        private readonly PagedEntityFetcher<Category, CategoryFilter> _pagedFetcher;

        public CategoryRetriever(CategoryClient categoryClient, ILogger<CategoryRetriever> logger)
        {
            _categoryClient = categoryClient;
            _pagedFetcher = new PagedEntityFetcher<Category, CategoryFilter>(FetchPageAsync, logger);
        }

        public Task<Result<IList<Category>>> RetrieveAsync(CategoryFilter filter) => _pagedFetcher.RetrieveAllAsync(filter);

        private async Task<Result<EntityPage<Category>>> FetchPageAsync(CategoryFilter filter, int offset)
        {
            GetCategoriesRequest categoriesRequest = new()
            {
                Offset = offset,
                Limit = filter.PageLength
            };

            Result<GetCategoriesResponse> response = await _categoryClient.GetAsync(categoriesRequest);

            if (response.IsFailed)
                return Result.Fail(response.Errors);

            return Result.Ok(new EntityPage<Category>(response.Value.Categories, response.Value.Pagination));
        }
    }
}
