using System;

namespace Auction.Mvc.Models
{
    public class ProductDto
    {
        
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int CategoryId { get; set; }

        public decimal StartBid { get; set; }
        
        public decimal Bid { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime RemainingTime { get; set; }
    }

    public class ProductCreateDto
    {
        public int CategoryId { get; set; }
        public string Category { get; set; }
        public string Market { get; set; }
    }

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
    public class ProductMarketDto
    {

        public Guid Id { get; set; }

        public Guid MarketId { get; set; }

        public Guid ProductId { get; set; }
    }
}