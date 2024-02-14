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
            SaveData(_auctionsKey, auctions);
        }

        public AuctionModel? GetAuction(string friendlyId)
        {
            var auctions = GetCurrentAuctions();
            return auctions.FirstOrDefault(x => x.Id.ToUpper() == friendlyId.ToUpper());
        }

        public void AddBid(AuctionBid bid)
        {
            var auction = GetCurrentAuctions().First(x => x.Id == bid.AuctionId);
            auction.Bids.Add(bid);
        }

        public void UpdateAuction(AuctionModel auction)
        {
            var auctions = GetCurrentAuctions();
            var old = auctions.First(x => x.Id == auction.Id);
            var index = auctions.IndexOf(old);

            if (index != -1)
            {
                auctions[index] = auction;
            }

            SaveData(_auctionsKey, auctions);
        }

        public List<AuctionModel> GetCurrentAuctions()
            => _cache.Get<List<AuctionModel>>(_auctionsKey) ?? [];

        private void SaveData<T>(string key, T value)
            => _cache.Set(key, value, TimeSpan.FromDays(1));
    }
}
