using Auction_web.Areas.Admin.Models;
using Auction_web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bids = await _context.Bids.ToListAsync();

            var model = new ReportsViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                BlockedUsers = await _context.Users.CountAsync(u => u.IsBlocked),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalItems = await _context.Items.CountAsync(),
                ActiveItems = await _context.Items.CountAsync(i => i.BidStatus == "A"),
                ClosedItems = await _context.Items.CountAsync(i => i.BidStatus != "A"),
                TotalBids = bids.Count,
                HighestBid = bids.Any() ? bids.Max(b => b.BidAmount) : 0,
                TotalBidValue = bids.Sum(b => b.BidAmount)
            };

            return View(model);
        }
    }
}