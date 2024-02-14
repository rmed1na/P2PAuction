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

        public AuctionRequestHandler(PeerModel authorPeer)
        {
            _authorPeer = authorPeer;
        }

        /// <summary>
        /// Initializes a new auction
        /// </summary>
        /// <param name="item">The item to be set into auction</param>
        /// <param name="price">The item price set by the auction author</param>
        /// <param name="author">The author or creator of this auction</param>
        public async void Initialize(string item, double price, string author)
        {
            var auctionId = Guid.NewGuid().ToString()[30..].ToUpper();
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                var client = new AuctionHandler.AuctionHandlerClient(channel);
                client.Initialize(new AuctionData
                {
                    AuctionId = auctionId,
                    Item = item,
                    Price = price,
                    Author = author
                });

                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        /// Places a new bid into an auction
        /// </summary>
        /// <param name="auctionId">Auction unique identifier</param>
        /// <param name="amount">Amount of the bid to be sent</param>
        /// <param name="author">Author sending the new bid</param>
        public async void PlaceBid(string auctionId, double amount, string author)
        {
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                var client = new AuctionHandler.AuctionHandlerClient(channel);
                client.PlaceBid(new BidData
                {
                    AuctionId = auctionId,
                    Amount = amount,
                    Bidder = author
                });

                await channel.ShutdownAsync();
            }
        }

        /// <summary>
        /// Completes/closes an open auction
        /// </summary>
        /// <param name="auction">Auction unique identifier</param>
        /// <param name="highestBid">Highest bid registry</param>
        public async void Complete(AuctionModel auction, AuctionBid highestBid)
        {
            foreach (var connectedPeer in _authorPeer.ConnectedPeers)
            {
                var channel = new Channel(connectedPeer.Key, ChannelCredentials.Insecure);
                var client = new AuctionHandler.AuctionHandlerClient(channel);
                client.Complete(new CompletionData
                {
                    AuctionId = auction.Id,
                    HighestBidder = highestBid.Bidder,
                    Price = highestBid.Amount
                });

                await channel.ShutdownAsync();
            }
        }
    }
}
