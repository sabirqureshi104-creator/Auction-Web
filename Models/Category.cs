    using System.ComponentModel.DataAnnotations;
    namespace Auction_web.Models
    {
        public class Category
        {
            [Key]
            public int CategoryId { get; set; }

            [Required(ErrorMessage = "Category name is required")]
            [StringLength(100)]
            public string CategoryName { get; set; }

            [StringLength(500)]
            public string Description { get; set; }

            public DateTime CreatedDate { get; set; } = DateTime.Now;

            // Navigation Property
            public ICollection<Item> Items { get; set; } = new List<Item>();
        }
    }
