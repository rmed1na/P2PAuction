using Auction.Data.Models;
using Auction.Data.Repositories;
using Grpc.Core;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Services
{
    public class AuctionService : AuctionHandler.AuctionHandlerBase
    {
        private readonly IAuctionRepository _auctionRepository;

        public AuctionService(IAuctionRepository auctionRepository)
        {
            _auctionRepository = auctionRepository;
        }

        public override async Task<AuctionData> Initialize(AuctionData request, ServerCallContext context)
        {
            Console.WriteLine($"{request.UserName} initialized auction {request.AuctionId} for item '{request.Item}' at ${request.Price}");

            var auction = new AuctionModel
            {
                Item = request.Item,
                Price = request.Price,
                Author = request.UserName,
                FriendlyId = request.AuctionId
            };

            _auctionRepository.AddAuction(auction);

            return request;
        }

        public override async Task<BidData> PlaceBid(BidData request, ServerCallContext context)
        {
            Console.WriteLine($"{request.Bidder} has placed a new bid for {request.Amount} in auction {request.AuctionId}");

            _auctionRepository.AddBid(new AuctionBid
            {
                AuctionFriendlyId = request.AuctionId,
                Amount = request.Amount,
                Bidder = request.Bidder
            });

            return request;
        }
    }
}
