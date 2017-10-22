using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Models
{
    public class UserDto
    {
        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public string CategoryName { get; set; }

        public decimal CurrentBid { get; set; }
    }
}