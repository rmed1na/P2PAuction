using Auction.Application.Auction;
using Auction.Data.Models;
using Auction.Data.Repositories;

namespace Auction
{
    public class Menu
    {
        private readonly Peer _peer;
        private readonly IPeerRepository _peerRepository;
        private readonly AuctionRequestHandler _auctionRequestHandler;
        private readonly IAuctionRepository _auctionRepository;

        public Menu(
            Peer peer,
            IPeerRepository peerRepository,
            IAuctionRepository auctionRepository)
        {
            _peer = peer;
            _peerRepository = peerRepository;
            _auctionRepository = auctionRepository;
            _auctionRequestHandler = new AuctionRequestHandler(_peer, _auctionRepository);
        }

        public void Start()
        {
            string? command;
            do
            {
                Console.WriteLine($"\n[{_peer.Name}] [Connected peers: {_peer.ConnectedPeers.Count} @ {DateTime.Now}]");
                Console.WriteLine("Choose an action command: ");
                command = Console.ReadLine();

                switch (command)
                {
                    case "auction -i":
                        AuctionInitialize();
                        break;
                    case "auction -b":
                        AuctionBid();
                        break;
                    case "auction -c":
                        AuctionComplete();
                        break;

                    // From here, mostly for debugging purposes.
                    // Not really needed for the actual work of the auction system but nice-to-have.
                    case "auction -l":
                        AuctionList();
                        break;
                    case "peer -l":
                        PeersList();
                        break;
                    default:
                        break;
                }

            } while (!string.IsNullOrEmpty(command) && !command.Equals("exit"));
        }

        private void AuctionInitialize()
        {
            Console.Write("\nEnter the item: ");
            var item = Console.ReadLine();

            if (string.IsNullOrEmpty(item))
            {
                Console.Write("Invalid item description/title");
                return;
            }

            Console.Write("Enter the price: ");
            var priceStr = Console.ReadLine();

            if (!double.TryParse(priceStr, out double price))
            {
                Console.Write("Invalid price");
                return;
            }

            _auctionRequestHandler.Initialize(item, price, _peer.Name);
        }

        private void AuctionBid()
        {
            Console.Write("Enter the auction id: ");
            var auctionId = Console.ReadLine() ?? string.Empty;
            var auction = _auctionRepository.GetAuction(auctionId);

            if (auction == null || string.IsNullOrEmpty(auctionId))
            {
                Console.WriteLine("Invalid auction id");
                return;
            }

            if (auction.Status != AuctionStatusCode.Open)
            {
                Console.WriteLine($"Auction {auction.Id} is already closed");
                return;
            }

            if (auction.Author.Equals(_peer.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("You're the auction author. The author can't bid in his/her own auction. Play fair :)");
                return;
            }

            Console.Write("Enter the bid amount: ");

            if (!double.TryParse(Console.ReadLine(), out double bidAmount))
            {
                Console.WriteLine("Invalid bid amount");
                return;
            }

            _auctionRequestHandler.PlaceBid(auctionId, bidAmount, _peer.Name);
        }

        private void AuctionComplete()
        {
            Console.Write("Enter the auction id: ");
            var auctionId = Console.ReadLine() ?? string.Empty;
            var auction = _auctionRepository.GetAuction(auctionId);

            if (auction == null)
            {
                Console.WriteLine("Invalid auction id");
                return;
            }

            var highestBid = auction.GetHighestBid();

            if (highestBid == null)
            {
                Console.WriteLine($"Auction {auctionId} doesn't have any bids yet");
                return;
            }

            Console.WriteLine($"Auction {auctionId}'s highest bidder is {highestBid.Bidder} with {highestBid.Amount} for product '{auction.Item}'");
            Console.Write("Do you wish to complete the auction now? (Y/N) > ");

            var answer = Console.ReadLine();
            var acceptedAnswers = new string[] { "Y", "N" };

            if (string.IsNullOrEmpty(answer) || !acceptedAnswers.Contains(answer.ToUpper()))
            {
                Console.WriteLine("Invalid answer");
                return;
            }

            if (answer.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
            {
                _auctionRequestHandler.Complete(auction, highestBid);
            }
            else if (answer.Equals("N", StringComparison.InvariantCultureIgnoreCase))
            {
                Console.WriteLine("Bidding continues...");
            }
        }

        private void AuctionList()
        {
            var auctions = _auctionRepository.GetCurrentAuctions();
            Console.WriteLine("List of current auctions: ");
            foreach (var auction in auctions)
            {
                Console.WriteLine($" -> ID: {auction.Id} | Author: {auction.Author} | Item: {auction.Item} | Bids: {auction.Bids.Count} | Status: {auction.Status}");
            }
        }

        private void PeersList()
        {
            Console.WriteLine("List of connected peers: ");
            foreach (var peer in _peer.ConnectedPeers)
            {
                Console.WriteLine($" -> {peer.Value} @ {peer.Key}");
            }
        }
    }
}
