using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Core.Models
{
    public class ExternalEntityFilter
    {
        public int PageLength { get; set; }

        /// <summary>
        /// When set, only entities updated on or after this instant are retrieved.
        /// Used to implement delta synchronization against the external provider.
        /// </summary>
        public DateTime? MinimumUpdatedAt { get; set; }
    }
}
