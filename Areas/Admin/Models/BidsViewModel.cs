using Auction_web.Models;

namespace Auction_web.Areas.Admin.Models
{
    public class BidsViewModel
    {
        public string? Search { get; set; }
        public List<Bid> Bids { get; set; } = new();
    }
}