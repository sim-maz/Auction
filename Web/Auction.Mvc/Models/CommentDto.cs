using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Auction.Mvc.Models
{
    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
        public DateTime Time { get; set; }
    }
}