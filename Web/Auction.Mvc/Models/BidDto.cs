using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Models
{
    public class BidDto
    {
        public Guid Id { get; set; }

        public decimal Sum { get; set; }

        public Guid ProductId { get; set; }

        public DateTime BidTime { get; set; }

    }
}