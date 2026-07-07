using System;
using System.Collections.Generic;

namespace Auction_web.Models
{
    public class UserProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime JoinDate { get; set; }

        public int ProductsListedCount { get; set; }
        public int BidsPlacedCount { get; set; }
        public int AuctionsWonCount { get; set; }
        public decimal TotalEarnings { get; set; }

        // Agar aapke paas Items aur Bids ki classes hain toh unki lists yahan aayengi
        public IEnumerable<dynamic> MyProducts { get; set; } = new List<dynamic>();
        public IEnumerable<dynamic> MyBids { get; set; } = new List<dynamic>();
    }
}