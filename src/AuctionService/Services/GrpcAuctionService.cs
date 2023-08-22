using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuctionService.Data;
using Grpc.Core;

namespace AuctionService.Services
{
    public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
    {
        private readonly AuctionDbContext _dbContext;
        public GrpcAuctionService(AuctionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<GetAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
        {
            System.Console.WriteLine("==> Received Grpc request for auction");

            var auction = await _dbContext.Auctions.FindAsync(Guid.Parse(request.Id)) 
                         ?? throw new RpcException(new Status(StatusCode.NotFound, "Not found"));

            var response = new GetAuctionResponse{
                Auction = new GrpcAuctionModel{
                    AuctionEnd = auction.AuctionEnd.ToString(),
                    Id = auction.Id.ToString(),
                    ReservedPrice = auction.ReservePrice,
                    Seller = auction.Seller
                }
            };

            return response;
        }

    }
}