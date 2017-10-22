using System;
using System.ComponentModel.DataAnnotations;

namespace Auction.Domain
{
    public class Comment
    {
        public Comment()
        {
            Id = Guid.NewGuid();
            Time = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid ProductId { get; set; }

        [StringLength(512)]
        [Required]
        public string Text { get; set; }
        [StringLength(50)]
        public string User { get; set; }
        public DateTime Time { get; set; }
    }
}
