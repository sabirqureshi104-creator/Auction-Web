using Auction_web.Models;

namespace Auction_web.Areas.Admin.Models
{
    public class ItemsViewModel
    {
        public string? Search { get; set; }

        public List<Item> Items { get; set; } = new();
    }
}