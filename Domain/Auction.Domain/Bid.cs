using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auction.Domain
{
    public class Bid
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Enter a bid")]
        [Range(1,999999,ErrorMessage ="The bid must be more than 0") ]
        public decimal Sum { get; set; }

        public Guid ProductId { get; set; }

        public DateTime BidTime { get; set; }

        public string User { get; set; }

        public bool Winner { get; set; }
    }
}
