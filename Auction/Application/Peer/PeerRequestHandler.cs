using Auction.Data.Repositories;
using Grpc.Core;
using PeerModel = Auction.Data.Models.Peer;

namespace Auction.Application.Peer
{
    public class PeerRequestHandler
    {
        /// <summary>
        /// Pings a fellow peer known by it's address.
        /// </summary>
        /// <param name="requestingPeer">Requesting peer</param>
        /// <param name="fellowPeerPort">Fellow peer port</param>
        /// <returns></returns>
        public async Task PingFellowPeer(PeerModel requestingPeer, int fellowPeerPort)
        {
            var requestingKnownPeer = new KnownPeer
            {
                Address = requestingPeer.Address,
                Name = requestingPeer.Name
            };

            var channel = new Channel($"localhost:{fellowPeerPort}", ChannelCredentials.Insecure);
            var peerClient = new PeerHandler.PeerHandlerClient(channel);
            var connectedPeers = peerClient.Ping(new PingRequest
            {
                RequestingPeer = requestingKnownPeer
            });

            foreach (var connectedPeer in connectedPeers.KnownPeers)
            {
                requestingPeer.AddConnectedPeerIfNotExists(connectedPeer.Address, connectedPeer.Name);

                var peerChannel = new Channel(connectedPeer.Address, ChannelCredentials.Insecure);
                var fellowPeerClient = new PeerHandler.PeerHandlerClient(peerChannel);

                fellowPeerClient.Ping(new PingRequest
                {
                    RequestingPeer = requestingKnownPeer
                });

                await peerChannel.ShutdownAsync();
            }

            await channel.ShutdownAsync();
        }
    }
}
