using Auction_web.Models;

namespace Auction_web.Areas.Admin.Models
{
    public class UsersViewModel
    {
        public string? Search { get; set; }
        public List<User> Users { get; set; } = new();
    }
}