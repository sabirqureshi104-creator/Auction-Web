using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Auction_web.Models;
using Auction_web.Data;
// TODO: Agar aapke Data context ka folder different hai to uska using name lagayein yahan (e.g., using Auction_web.Data;)

namespace Auction_web.Services
{
    public class AuctionTimeoutWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15); // Har 15 seconds mein trigger hoga

        public AuctionTimeoutWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        // TODO: Apni exact DbContext Class ka naam yahan 'AuctionDbContext' ki jagah replace karein agar different hai
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var expiredItems = await context.Items
                            .Include(i => i.Bids)
                            .Where(i => i.BidStatus == "A" && i.AuctionEndDate <= DateTime.Now)
                            .ToListAsync(stoppingToken);

                        foreach (var item in expiredItems)
                        {
                            var highestBid = item.Bids
                                .OrderByDescending(b => b.BidAmount)
                                .FirstOrDefault();

                            if (highestBid != null)
                            {
                                item.BidStatus = "S"; // S = Sold
                                item.WinnerId = highestBid.BuyerId;

                                // Notification for Seller
                                context.Notifications.Add(new Notification
                                {
                                    Message = $"Your item '{item.ItemTitle}' has been sold for ${highestBid.BidAmount}.",
                                    UserId = item.SellerId,
                                    ItemId = item.ItemId,
                                    CreatedDate = DateTime.Now
                                });

                                // Notification for Winner
                                context.Notifications.Add(new Notification
                                {
                                    Message = $"You won the auction for '{item.ItemTitle}' with a bid of ${highestBid.BidAmount}.",
                                    UserId = highestBid.BuyerId,
                                    ItemId = item.ItemId,
                                    CreatedDate = DateTime.Now
                                });
                            }
                            else
                            {
                                item.BidStatus = "E"; // E = Expired
                                context.Notifications.Add(new Notification
                                {
                                    Message = $"Your auction for '{item.ItemTitle}' ended with no bids.",
                                    UserId = item.SellerId,
                                    ItemId = item.ItemId,
                                    CreatedDate = DateTime.Now
                                });
                            }
                        }

                        if (expiredItems.Any())
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception)
                {
                    // Fail-safe logic
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}