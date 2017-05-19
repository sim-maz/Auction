using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction.Domain
{
    public class ProductMarket
    {
        public ProductMarket()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public Guid MarketId { get; set; }
        public Guid ProductId { get; set; }

    }
}
