namespace Auction.Data.Models
{
    public class Auction
    {
        public Guid Id { get; set; }
        public string? FriendlyId { get; set; }
        public string? Item { get; set; }
        public double Price { get; set; }
        public string? Author { get; set; }
        public List<AuctionBid> Bids { get; set; } = [];

        public Auction() 
        {
            Id = Guid.NewGuid();
            FriendlyId = Id.ToString()[30..].ToUpper();
        }

        public Auction(string item, double price, string author)
        {
            Id = Guid.NewGuid();
            FriendlyId = Id.ToString()[30..].ToUpper();
            Item = item;
            Price = price;
            Author = author;
        }
    }
}
