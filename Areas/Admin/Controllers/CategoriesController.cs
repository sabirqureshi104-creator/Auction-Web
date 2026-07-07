using Auction_web.Areas.Admin.Models;
using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auction_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var query = _context.Categories
                .Include(c => c.Items)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(search) ||
                    (c.Description != null && c.Description.Contains(search)));
            }

            var model = new CategoriesViewModel
            {
                Search = search,
                Categories = await query
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string categoryName, string? description)
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
        public async Task<IActionResult> Edit(int id, string categoryName, string? description)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category != null)
            {
                category.CategoryName = categoryName;
                category.Description = description;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category != null && !category.Items.Any())
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}