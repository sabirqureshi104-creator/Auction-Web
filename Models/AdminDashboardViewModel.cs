using Auction_web.Models;

namespace Auction_web.Models
{
    public class AdminDashboardViewModel
    {
        public List<User> Users { get; set; } = new List<User>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Item> Items { get; set; } = new List<Item>();
        public List<Bid> Bids { get; set; } = new List<Bid>();

        public int TotalUsers => Users.Count;
        public int TotalBlockedUsers => Users.Count(u => u.IsBlocked);
        public int TotalCategories => Categories.Count;
        public int TotalActiveItems => Items.Count(i => i.BidStatus == "A" && i.AuctionEndDate > DateTime.Now);
        public int TotalClosedItems => Items.Count(i => i.BidStatus != "A" || i.AuctionEndDate <= DateTime.Now);
        public int TotalBidsToday => Bids.Count(b => b.BidDate.Date == DateTime.Today);
    }
}
