namespace Auction.Data.Models
{
    public class Auction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString()[30..].ToUpper();
        public string? Item { get; set; }
        public double Price { get; set; }
        public string? Author { get; set; }
        public AuctionStatusCode Status { get; set; } = AuctionStatusCode.Unknown;
        public List<AuctionBid> Bids { get; set; } = [];

        public Auction() { }

        public AuctionBid? GetHighestBid()
        {
            return Bids
                .OrderByDescending(x => x.Amount)
                .FirstOrDefault();
        }
    }
}
