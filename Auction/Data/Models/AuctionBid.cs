namespace Auction.Data.Models
{
    public class AuctionBid
    {
        public string? AuctionFriendlyId { get; set; }
        public double Amount { get; set; }
        public string? Bidder { get; set; }
    }
}
