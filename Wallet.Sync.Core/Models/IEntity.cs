using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Core.Models
{
    public interface IEntity
    {
        public string ExternalId { get; set; }
    }
}
