using Auction_web.Models;

namespace Auction_web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalItems { get; set; }
        public int ActiveAuctions { get; set; }
        public int TotalBids { get; set; }

        public List<User> RecentUsers { get; set; } = new();
        public List<Item> RecentItems { get; set; } = new();
        public List<Bid> RecentBids { get; set; } = new();
    }
}