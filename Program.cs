using Auction_web.Data;
using Auction_web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

builder.Services.AddHostedService<Auction_web.Services.AuctionTimeoutWorker>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Database seeding block (Sirf Categories aur Demo User ke liye)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { CategoryName = "Electronics", Description = "Gadgets and tech devices", CreatedDate = DateTime.Now },
            new Category { CategoryName = "Furniture", Description = "Home and office furniture", CreatedDate = DateTime.Now },
            new Category { CategoryName = "Books", Description = "Educational and story books", CreatedDate = DateTime.Now },
            new Category { CategoryName = "Clothing", Description = "Apparel and fashion items", CreatedDate = DateTime.Now },
            new Category { CategoryName = "Collectibles", Description = "Rare and premium items", CreatedDate = DateTime.Now }
        );
        context.SaveChanges();
    }

    if (!context.Users.Any())
    {
        context.Users.Add(new User
        {
            Username = "demoseller",
            Email = "seller@demo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            UserType = "User",
            IsBlocked = false,
            CreatedDate = DateTime.Now,
            FullName = "Demo Seller"
        });
        context.SaveChanges();
    }


    
}

app.Run();