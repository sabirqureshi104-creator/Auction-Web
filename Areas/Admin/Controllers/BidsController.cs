using Microsoft.AspNetCore.Authorization;
using Auction_web.Areas.Admin.Models;
using Auction_web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class BidsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BidsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Bids
                .Include(b => b.Item)
                .Include(b => b.Buyer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.Item.ItemTitle.Contains(search) ||
                    b.Buyer.Username.Contains(search) ||
                    b.Buyer.Email.Contains(search));
            }

            var model = new BidsViewModel
            {
                Search = search,
                Bids = await query
                    .OrderByDescending(b => b.BidDate)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}