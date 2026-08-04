using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Core.Services
{
    public interface IExternalEntityRetriever<T, U> 
        where T : class
        where U : ExternalEntityFilter
    {
        public Task<Result<IList<T>>> RetrieveAsync(U filter);
    }
}
