using System;
using System.ComponentModel.DataAnnotations;

namespace Auction.Domain
{
    public class Product
    {
        

        public Product()
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
        }

        public Guid Id { get; set; }
        [Required(ErrorMessage = "A Product Name is required")]
        [StringLength(160)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please enter a starting bid")]
        public decimal StartBid { get; set; }

        public DateTime StartTime { get; set; }

        public double Duration { get; set; }

        public DateTime EndTime { get; set; }
    }
}
