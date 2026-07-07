using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_web.Models
{
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item title is required")]
        [StringLength(100)]
        public string ItemTitle { get; set; }

        [StringLength(4000)]
        public string ItemDescription { get; set; }

        [StringLength(255)]
        public string ImagePath { get; set; }

        [StringLength(255)]
        public string DocumentPath { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MinimumBid { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? CurrentBid { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? BidIncrement { get; set; }

        [StringLength(1)]
        public string BidStatus { get; set; } = "A"; // A=Active, I=Inactive

        public DateTime AuctionStartDate { get; set; }

        public DateTime AuctionEndDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [ForeignKey("Seller")]
        public int SellerId { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        [ForeignKey("Winner")]
        public int? WinnerId { get; set; } // Added for tracking winner

        // Navigation Properties
        public User Seller { get; set; }

        public User Winner { get; set; } // Added navigation
        public Category Category { get; set; }
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
