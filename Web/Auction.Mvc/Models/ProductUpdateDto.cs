using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Models
{
    public class ProductUpdateDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int CategoryId { get; set; }

        public decimal StartBid { get; set; }

        public DateTime StartTime { get; set; }

        public double Duration { get; set; }

        public DateTime EndTime { get; set; }

    }
}