using Auction_web.Models;

namespace Auction_web.Areas.Admin.Models
{
    public class CategoriesViewModel
    {
        public string? Search { get; set; }
        public List<Category> Categories { get; set; } = new();
    }
}