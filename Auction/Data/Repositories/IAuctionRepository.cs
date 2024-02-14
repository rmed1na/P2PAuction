using Auction.Data.Models;
using AuctionModel = Auction.Data.Models.Auction;

namespace Auction.Data.Repositories
{
    public interface IAuctionRepository
    {
        public void AddAuction(AuctionModel auction);
        public AuctionModel? GetAuction(string friendlyId);
        public void AddBid(AuctionBid bid);
        public void UpdateAuction(AuctionModel auction);
        public List<AuctionModel> GetCurrentAuctions();
    }
}