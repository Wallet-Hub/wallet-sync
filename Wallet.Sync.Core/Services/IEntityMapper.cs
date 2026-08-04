using System;
using System.Collections.Generic;
using System.Text;
using Wallet.Sync.Core.Models;

namespace Wallet.Sync.Core.Services
{
    public interface IEntityMapper<TSource, TDestination>
        where TSource : class
        where TDestination : class, IEntity
    {
        public TDestination Map(TSource source);
    }
}
