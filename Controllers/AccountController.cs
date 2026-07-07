using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Auction_web.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // REGISTER GET (SECURE)
        // ==========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            PreventPageCaching();
            return View();
        }

        // ==========================================
        // REGISTER POST (SECURE & PARAMETERIZED)
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string Username, string Email, string Password, string confirmPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Email) ||
                    string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(confirmPassword))
                {
                    ViewBag.Error = "All fields are required.";
                    return View();
                }

                if (Password != confirmPassword)
                {
                    ViewBag.Error = "Passwords do not match.";
                    return View();
                }

                if (Password.Length < 6)
                {
                    ViewBag.Error = "Password must be at least 6 characters long.";
                    return View();
                }

                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == Username.Trim());

                if (existingUser != null)
                {
                    ViewBag.Error = "Username already taken";
                    return View();
                }

                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email.Trim().ToLower());

                if (existingEmail != null)
                {
                    ViewBag.Error = "Email already registered";
                    return View();
                }

                string secureHashedPassword = BCrypt.Net.BCrypt.HashPassword(Password);

                var newUser = new User
                {
                    Username = Username.Trim(),
                    Email = Email.Trim().ToLower(),
                    PasswordHash = secureHashedPassword,
                    UserType = "User",
                    IsBlocked = false,
                    CreatedDate = DateTime.Now,
                    FullName = "",
                    PhoneNumber = "",
                    Address = "",
                    City = "",
                    Country = "",
                    ProfileImage = ""
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ViewBag.Error = "An unexpected error occurred during registration. Please try again.";
                return View();
            }
        }

        // ==========================================
        // LOGIN GET (SECURE)
        // ==========================================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            PreventPageCaching();
            return View();
        }

        // ==========================================
        // LOGIN POST (SECURE)
        // ==========================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
                {
                    ViewBag.Error = "Please enter both Email and Password.";
                    return View();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == Email.Trim().ToLower());

                if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                {
                    ViewBag.Error = "Invalid email or password";
                    return View();
                }

                if (user.IsBlocked)
                {
                    ViewBag.Error = "Your account is blocked. Please contact support.";
                    return View();
                }

                user.LastLogin = DateTime.Now;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.UserType)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ViewBag.Error = "Authentication error. Please try again later.";
                return View();
            }
        }

        // ==========================================
        // LOGOUT (SECURE POST METHOD)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            PreventPageCaching();
            return RedirectToAction("Index", "Home");
        }

        private void PreventPageCaching()
        {
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";
        }

        
    }
}