using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_web.Models
{
    public class Rating
    {

        [Key]
        public int RatingId { get; set; }

        [Range(-5, 5, ErrorMessage = "Rating must be between -5 and +5")]
        public int? RatingValue { get; set; }

        [StringLength(500)]
        public string ReviewText { get; set; }

        public DateTime RatedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [ForeignKey("Item")]
        public int ItemId { get; set; }

        [ForeignKey("RatedUser")]
        public int RatedUserId { get; set; }

        // Navigation Properties
        public Item Item { get; set; }
        public User RatedUser { get; set; }
    }
}
