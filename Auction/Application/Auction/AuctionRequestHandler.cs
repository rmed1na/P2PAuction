using Auction.Data.Models;
using Auction.Data.Repositories;
using Grpc.Core;
using AuctionModel = Auction.Data.Models.Auction;
using PeerModel = Auction.Data.Models.Peer;

namespace Auction.Application.Auction
{
    public class AuctionRequestHandler
    {
        private readonly PeerModel _authorPeer;
        private readonly IAuctionRepository _auctionRepository;

        public AuctionRequestHandler(PeerModel authorPeer, IAuctionRepository auctionRepository)
        {
            _authorPeer = authorPeer;
            _auctionRepository = auctionRepository;
        }

        public void Initialize(string item, double price, string author)
        {
            var auction = new AuctionModel(item, price, author);
            _auctionRepository.AddAuction(auction);

            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                var client = new AuctionHandler.AuctionHandlerClient(channel);
                client.Initialize(new AuctionData
                {
                    AuctionId = auction.FriendlyId,
                    Item = auction.Item,
                    Price = auction.Price,
                    UserName = auction.Author
                });
            }
        }

        public void SetBid(string auctionFriendlyId, double amount, string author)
        {
            var bid = new AuctionBid
            {
                AuctionFriendlyId = auctionFriendlyId,
                Amount = amount,
                Bidder = author
            };

            _auctionRepository.AddBid(bid);

            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                var client = new AuctionHandler.AuctionHandlerClient(channel);
                client.PlaceBid(new BidData
                {
                    AuctionId = bid.AuctionFriendlyId,
                    Amount = bid.Amount,
                    Bidder = bid.Bidder
                });
            }
        }
    }
}
