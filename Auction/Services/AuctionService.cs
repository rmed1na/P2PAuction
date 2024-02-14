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

        public override async Task<AuctionData> Initialize(AuctionData data, ServerCallContext context)
        {
            Console.WriteLine($" >> {data.UserName} initialized auction {data.AuctionId} for item '{data.Item}' at ${data.Price}");

            var auction = new AuctionModel
            {
                Id = data.AuctionId,
                Item = data.Item,
                Price = data.Price,
                Author = data.UserName,
            };

            _auctionRepository.AddAuction(auction);

            return data;
        }

        public override async Task<BidData> PlaceBid(BidData data, ServerCallContext context)
        {
            Console.WriteLine($" >> {data.Bidder} has placed a new bid for {data.Amount} in auction {data.AuctionId}");

            _auctionRepository.AddBid(new AuctionBid
            {
                AuctionId = data.AuctionId,
                Amount = data.Amount,
                Bidder = data.Bidder
            });

            return data;
        }

        public override async Task<CompletionData> Complete(CompletionData data, ServerCallContext context)
        {
            Console.WriteLine($" >> Auction {data.AuctionId} has been completed. Highest bidder: {data.HighestBidder}. Amount: {data.Price}");

            var auction = _auctionRepository.GetAuction(data.AuctionId);

            if (auction != null)
            {
                auction.Status = AuctionStatusCode.Closed;
                _auctionRepository.UpdateAuction(auction);
            }

            return data;
        }
    }
}
