using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auction_web.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign Keys
        [ForeignKey("User")]
        public int UserId { get; set; }

        [ForeignKey("Item")]
        public int? ItemId { get; set; }


        // Navigation Properties
        public User User { get; set; }
        public Item Item { get; set; }
    }
}
