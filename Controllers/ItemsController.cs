using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auction_web.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ItemsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,ItemTitle,ItemDescription,MinimumBid,BidIncrement,AuctionEndDate,CategoryId,ImagePath,DocumentPath")] Item item, IFormFile imageFile, IFormFile documentFile)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            item.SellerId = currentUserId.Value;

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string imgFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(imgFolderPath))
                    {
                        Directory.CreateDirectory(imgFolderPath);
                    }

                    string uniqueImgName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                    string imgFilePath = Path.Combine(imgFolderPath, uniqueImgName);

                    using (var stream = new FileStream(imgFilePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    item.ImagePath = "images/" + uniqueImgName;
                    

                }
                else
                {
                    item.ImagePath = "images/placeholder-item.jpg";
                }

                if (documentFile != null && documentFile.Length > 0)
                {
                    string docFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                    if (!Directory.Exists(docFolderPath))
                    {
                        Directory.CreateDirectory(docFolderPath);
                    }

                    string uniqueDocName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(documentFile.FileName);
                    string docFilePath = Path.Combine(docFolderPath, uniqueDocName);

                    using (var stream = new FileStream(docFilePath, FileMode.Create))
                    {
                        await documentFile.CopyToAsync(stream);
                    }

                    item.DocumentPath = "documents/" + uniqueDocName;
                }
                else
                {
                    item.DocumentPath = "documents/no-specifications.docx";
                }

                item.BidStatus = "A";
                item.AuctionStartDate = DateTime.Now;
                item.CreatedDate = DateTime.Now;
                item.UpdatedDate = DateTime.Now;

                if (item.AuctionEndDate <= DateTime.Now)
                {
                    ModelState.AddModelError(nameof(item.AuctionEndDate), "Auction end date must be in the future.");
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
                    return View(item);
                }

                _context.Add(item);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = item.ItemId });
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .Include(i => i.Bids)
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (item == null) return NotFound();

            return View(item);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int ItemId, decimal BidAmount)
        {
            var currentBuyerId = GetCurrentUserId();
            if (!currentBuyerId.HasValue)
            {
                TempData["Error"] = "You must be logged in to place a bid.";
                return RedirectToAction("Login", "Account");
            }

            var item = await _context.Items.Include(i => i.Bids).FirstOrDefaultAsync(i => i.ItemId == ItemId);
            if (item == null) return NotFound();

            if (item.SellerId == currentBuyerId.Value)
            {
                TempData["Error"] = "You cannot place a bid on your own listing.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }

            if (item.BidStatus != "A")
            {
                TempData["Error"] = "This auction is no longer active.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }

            if (item.AuctionEndDate <= DateTime.Now)
            {
                TempData["Error"] = "This auction has ended.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }

            decimal minimumAllowed = (item.CurrentBid ?? item.MinimumBid ?? 0) + (item.BidIncrement ?? 0);

            if (BidAmount < minimumAllowed)
            {
                TempData["Error"] = $"Your bid must be at least ${minimumAllowed:N2}.";
                return RedirectToAction(nameof(Details), new { id = ItemId });
            }

            var newBid = new Bid
            {
                ItemId = ItemId,
                BuyerId = currentBuyerId.Value,
                BidAmount = BidAmount,
                BidDate = DateTime.Now
            };
            _context.Bids.Add(newBid);

            item.CurrentBid = BidAmount;
            item.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Your bid of ${BidAmount:N2} was placed successfully.";

            return RedirectToAction(nameof(Details), new { id = ItemId });
        }

        [HttpGet]
        public async Task<IActionResult> Category(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category == null) return NotFound();

            var categoryItems = await _context.Items
                .Include(i => i.Category)
                .Where(i => i.CategoryId == id && i.BidStatus == "A")
                .ToListAsync();

            ViewBag.CategoryName = category.CategoryName;
            return View(categoryItems);
        }

        // ==========================================
        // 1. EDIT/UPDATE (GET) ACTION
        // ==========================================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var item = await _context.Items.FindAsync(id);
            if (item == null) return NotFound();

            // Security Check: Sirf wahi seller edit kar sake jis ka item hai
            var currentUserId = GetCurrentUserId();
            if (item.SellerId != currentUserId)
            {
                return Unauthorized();
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", item.CategoryId);
            return View(item);
        }

        // ==========================================
        // 2. EDIT/UPDATE (POST) ACTION
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Item item, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                var existingItem = _context.Items.Find(item.ItemId);

                // Image logic
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine("wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    existingItem.ImagePath = "images/" + fileName;
                }

                // Baki properties update karein
                existingItem.ItemTitle = item.ItemTitle;
                existingItem.ItemDescription = item.ItemDescription;
                existingItem.MinimumBid = item.MinimumBid;
                existingItem.BidIncrement = item.BidIncrement;
                existingItem.AuctionEndDate = item.AuctionEndDate;

                _context.Update(existingItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id = item.ItemId });
            }
            return View(item);
        }

        // ==========================================
        // 3. SECURE DELETE (POST) ACTION
        // ==========================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Items.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Item not found.";
                return RedirectToAction("Index", "Home");
            }

            // Security Check
            var currentUserId = GetCurrentUserId();
            if (item.SellerId != currentUserId)
            {
                return Unauthorized();
            }

            try
            {
                // Cascade Delete: Pehle saari bids remove karein takay SQL dependency crash na ho
                var linkedBids = _context.Bids.Where(b => b.ItemId == id);
                _context.Bids.RemoveRange(linkedBids);

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Item successfully deleted!";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the item.";
            }

            return RedirectToAction("Index", "Home");
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}