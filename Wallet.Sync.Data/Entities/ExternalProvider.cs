using System;
using System.Collections.Generic;
using System.Text;

namespace Wallet.Sync.Data.Entities
{
    public class ExternalProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
