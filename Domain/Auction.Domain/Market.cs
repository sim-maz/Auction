using System;
using System.ComponentModel.DataAnnotations;

namespace Auction.Domain
{
    public class Market
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "A Market Name is required")]
        public string MarketName { get; set; }

        public DateTime MarketStart { get; set; }

        public DateTime MarketEnd { get; set; }

        public int MarketType { get; set; }

        public int MarketStatus { get; set; }
    }
}
