using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentResults;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Core.Services
{
    public class MappingEntityRetriever<TExternalEntity, TFilter, TEntity> : IExternalEntityRetriever<TEntity, TFilter>
        where TExternalEntity : class
        where TFilter : ExternalEntityFilter
        where TEntity : class, IEntity
    {
        private readonly IExternalEntityRetriever<TExternalEntity, TFilter> _externalEntityRetriever;
        private readonly IEntityMapper<TExternalEntity, TEntity> _entityMapper;

        public MappingEntityRetriever(
            IExternalEntityRetriever<TExternalEntity, TFilter> externalEntityRetriever,
            IEntityMapper<TExternalEntity, TEntity> entityMapper)
        {
            _externalEntityRetriever = externalEntityRetriever;
            _entityMapper = entityMapper;
        }

        public async Task<Result<IList<TEntity>>> RetrieveAsync(TFilter filter)
        {
            Result<IList<TExternalEntity>> result = await _externalEntityRetriever.RetrieveAsync(filter);

            if (result.IsFailed)
                return Result.Fail(result.Errors);

            IList<TEntity> mappedEntities = result.Value
                .Select(_entityMapper.Map)
                .ToList();

            return Result.Ok(mappedEntities);
        }
    }
}
