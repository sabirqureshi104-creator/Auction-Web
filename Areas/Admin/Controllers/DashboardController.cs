using Microsoft.AspNetCore.Authorization;
using Auction_web.Areas.Admin.Models;
using Auction_web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalItems = await _context.Items.CountAsync(),
                ActiveAuctions = await _context.Items.CountAsync(x => x.BidStatus == "A"),
                TotalBids = await _context.Bids.CountAsync(),

                RecentUsers = await _context.Users
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .ToListAsync(),

                RecentItems = await _context.Items
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(5)
                    .ToListAsync(),

                RecentBids = await _context.Bids
                    .Include(x => x.Item)
                    .Include(x => x.Buyer)
                    .OrderByDescending(x => x.BidDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}