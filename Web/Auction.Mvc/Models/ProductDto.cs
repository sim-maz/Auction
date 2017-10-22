using System;

namespace Auction.Mvc.Models
{
    public class ProductDto
    {

        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string Category { get; set; }

        public int CategoryId { get; set; }

        public decimal StartBid { get; set; }

        public decimal Bid { get; set; }

        public string ImgPath { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime RemainingTime { get; set; }
    }  
}