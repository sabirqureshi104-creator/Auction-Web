using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
namespace Auction_web.Models
{
    public class User 
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [StringLength(255)]
        public string ProfileImage { get; set; }

        public bool IsBlocked { get; set; } = false;

        [StringLength(20)]
        public string UserType { get; set; } = "User"; // "Admin" or "User"

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        // Navigation Properties
        public ICollection<Item> ItemsForSale { get; set; } = new List<Item>();
        public ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
        public ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();


    }
}
