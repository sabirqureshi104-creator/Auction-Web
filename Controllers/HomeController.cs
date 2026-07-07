using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Auction_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            // 1. Total Volume calculation (Active auctions ke prices ka sum)
            var activeBidsSum = await _context.Items
                .Where(i => i.BidStatus == "A")
                .SumAsync(i => (double?)i.CurrentBid) ?? 0.0;

            // 2. Base items query for searching and displaying
            var itemsQuery = _context.Items
                .Include(i => i.Category)
                .Where(i => i.BidStatus == "A");

            // 3. Search Filter Logic (Title aur Description dono par check)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var pattern = "%" + searchString.Trim() + "%";
                itemsQuery = itemsQuery.Where(i =>
                    EF.Functions.Like(i.ItemTitle, pattern) ||
                    (i.ItemDescription != null && EF.Functions.Like(i.ItemDescription, pattern)));
            }

            var activeItems = await itemsQuery
                .OrderByDescending(i => i.ItemId)
                .ToListAsync();

            // 4. Fetch Real Categories from DB directly into standard Category Model
            var categoriesData = await _context.Categories
                .Include(c => c.Items)
                .ToListAsync();

            // 5. Total counts and dynamic calculations for stats grid
            var totalUsersCount = await _context.Users.CountAsync();
            var liveAuctionsCount = await _context.Items.CountAsync(i => i.BidStatus == "A");

            // 6. Binding everything to ViewBag to perfectly sync with Index.cshtml
            ViewBag.Categories = categoriesData;
            ViewBag.CurrentSearch = searchString;
            ViewBag.LiveAuctionsCount = liveAuctionsCount;
            ViewBag.ActiveBidders = totalUsersCount > 0 ? totalUsersCount + "K+" : "0K+";
            ViewBag.TotalVolume = activeBidsSum > 0
                ? (activeBidsSum / 1000000).ToString("F1") + "M+"
                : "0.0M+";

            // Direct database Item collection model pass ho raha hai bina kisi View Model ke
            return View(activeItems);
        }
    }
}