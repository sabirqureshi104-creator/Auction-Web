using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_web.Models
{
    public class Bid
    {
        [Key]
        public int BidId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Required]
        public decimal BidAmount { get; set; }

        public DateTime BidDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [ForeignKey("Item")]
        public int ItemId { get; set; }

        [ForeignKey("Buyer")]
        public int BuyerId { get; set; }

        // Navigation Properties
        public Item Item { get; set; }
        public User Buyer { get; set; }
    }
}
