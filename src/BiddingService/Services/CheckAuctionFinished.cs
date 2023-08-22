using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MassTransit.Testing;
using MongoDB.Entities;

namespace BiddingService.Services
{
    public class CheckAuctionFinished : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CheckAuctionFinished> _logger;

        public CheckAuctionFinished(ILogger<CheckAuctionFinished> logger, IServiceProvider services)
        {
            this._logger = logger;
            this._services = services;            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogInformation("Starting check for finished auctions.");

            stoppingToken.Register(() => _logger.LogInformation("==> Auction check is stopping"));

            while(!stoppingToken.IsCancellationRequested)
            {
                await CheckAuctions(stoppingToken);

                await Task.Delay(5000, stoppingToken);
            }
        }

        private async Task CheckAuctions(CancellationToken stoppingToken)
        {
            var finishedAuctions = await DB.Find<Auction>()
                                .Match(x => x.AuctionEnd <= DateTime.UtcNow)
                                .Match(x => !x.Finished)
                                .ExecuteAsync(stoppingToken);

            if(finishedAuctions.Count == 0) return;

            _logger.LogInformation("==> Found {count} auction that have completed", finishedAuctions.Count);

            using var scope = _services.CreateScope();
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            foreach(var finishedAuction in finishedAuctions)
            {
                finishedAuction.Finished = true;
                await finishedAuction.SaveAsync(null,stoppingToken);
                
                var winningBid = await DB.Find<Bid>()
                                .Match(x =>x.AuctionId == finishedAuction.ID)
                                .Match(x =>x.BidStatus == BidStatus.Accepted)
                                .Sort(x =>x.Descending(s => s.Amount))
                                .ExecuteFirstAsync(stoppingToken);
                await endpoint.Publish(new AuctionFinished{
                    ItemSold = winningBid != null,
                    AuctionId = finishedAuction.ID,
                    Winner = winningBid?.Bidder,
                    Amount = winningBid?.Amount,
                    Seller = finishedAuction.Seller
                }, stoppingToken);
            }
        }
    }
}