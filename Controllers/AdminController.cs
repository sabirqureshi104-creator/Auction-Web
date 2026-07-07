using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var usersQuery = _context.Users.AsQueryable();
            var itemsQuery = _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                usersQuery = usersQuery.Where(u =>
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    (u.FullName != null && u.FullName.Contains(search)));

                itemsQuery = itemsQuery.Where(i =>
                    i.ItemTitle.Contains(search) ||
                    (i.ItemDescription != null && i.ItemDescription.Contains(search)) ||
                    (i.Category != null && i.Category.CategoryName.Contains(search)) ||
                    (i.Seller != null && i.Seller.Username.Contains(search)));
            }

            var model = new AdminDashboardViewModel
            {
                Users = await usersQuery.OrderByDescending(u => u.CreatedDate).ToListAsync(),
                Categories = await _context.Categories.Include(c => c.Items).OrderBy(c => c.CategoryName).ToListAsync(),
                Items = await itemsQuery.OrderByDescending(i => i.CreatedDate).ToListAsync(),
                Bids = await _context.Bids
                    .Include(b => b.Item)
                    .Include(b => b.Buyer)
                    .OrderByDescending(b => b.BidDate)
                    .Take(50)
                    .ToListAsync()
            };

            ViewBag.Search = search;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsBlocked = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsBlocked = false;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string categoryName, string description)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                _context.Categories.Add(new Category
                {
                    CategoryName = categoryName.Trim(),
                    Description = description,
                    CreatedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeCategory(int sourceCategoryId, int targetCategoryId)
        {
            if (sourceCategoryId == targetCategoryId)
            {
                return RedirectToAction(nameof(Index));
            }

            var source = await _context.Categories.FindAsync(sourceCategoryId);
            var target = await _context.Categories.FindAsync(targetCategoryId);

            if (source != null && target != null)
            {
                var items = await _context.Items.Where(i => i.CategoryId == sourceCategoryId).ToListAsync();
                foreach (var item in items)
                {
                    item.CategoryId = targetCategoryId;
                }

                _context.Categories.Remove(source);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                item.BidStatus = "I";
                item.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
