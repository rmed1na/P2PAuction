using Auction.Data.Models;
using Microsoft.Extensions.Caching.Memory;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly IMemoryCache _cache;
        private readonly string _auctionsKey = "auctions";

        public AuctionRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddAuction(AuctionModel auction)
        {
            var auctions = GetCurrentAuctions();
            auctions.Add(auction);
            _cache.Set(_auctionsKey, auctions, TimeSpan.FromDays(1));
        }

        public AuctionModel? GetAuction(string friendlyId)
        {
            var auctions = GetCurrentAuctions();
            return auctions.FirstOrDefault(x => x.FriendlyId == friendlyId);
        }

        public void AddBid(AuctionBid bid)
        {
            var auction = GetCurrentAuctions().First(x => x.FriendlyId == bid.AuctionFriendlyId);
            auction.Bids.Add(bid);
        }

        private List<AuctionModel> GetCurrentAuctions()
            => _cache.Get<List<AuctionModel>>(_auctionsKey) ?? [];
    }
}
