using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace Auction_web.Controllers
{
    [Authorize] // Sirf login user access kar sake
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
     

        // Yahan (UserManager<User> userManager) ko as a parameter add karna zaroori hai
        public ProfileController(ApplicationDbContext context)
        {
            _context = context;

        }

        // Profile Index Page
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var userItems = await _context.Items.Where(i => i.SellerId == userId).ToListAsync();
            var userBids = await _context.Bids.Where(b => b.BuyerId == userId).Include(b => b.Item).ToListAsync();

            var viewModel = new UserProfileViewModel
            {
                UserName = user.Username,
                Email = user.Email,
                JoinDate = user.CreatedDate,
                MyProducts = userItems,
                MyBids = userBids
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);

            // Yahan 'EditProfileViewModel' ko initialize karein
            var model = new EditProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                Country = user.Country
            };

            return View(model);
        }

        // Edit Profile POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var user = await _context.Users.FindAsync(userId);

                if (user != null)
                {
                    // ViewModel se database object mein data dal rahe hain
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.City = model.City;
                    user.Country = model.Country;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync(); // Ye line database mein save karti hai

                    TempData["SuccessMessage"] = "Profile updated successfully!";
                    return RedirectToAction("Index");
                }
            }
            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

    }
}