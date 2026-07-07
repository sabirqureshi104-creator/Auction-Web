namespace Auction_web.Areas.Admin.Models
{
    public class ReportsViewModel
    {
        public int TotalUsers { get; set; }
        public int BlockedUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int ClosedItems { get; set; }
        public int TotalBids { get; set; }
        public decimal HighestBid { get; set; }
        public decimal TotalBidValue { get; set; }
    }
}