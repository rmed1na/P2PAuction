using Auction.Data.Repositories;
using Grpc.Core;
using PeerModel = Auction.Data.Models.Peer;

namespace Auction.Application.Peer
{
    public class PeerRequestHandler
    {
        private readonly IPeerRepository _peerRepository;

        public PeerRequestHandler(IPeerRepository peerRepository)
        {
            _peerRepository = peerRepository;
        }

        public async Task GetConnectedPeersAsync(PeerModel requestingPeer, int fellowPeerPort)
        {
            var channel = new Channel($"localhost:{fellowPeerPort}", ChannelCredentials.Insecure);
            var peerClient = new PeerHandler.PeerHandlerClient(channel);
            var connectedPeers = peerClient.GetConnectedPeers(new GetConnectedPeersRequest
            {
                RequestingPeer = new ConnectedPeer
                {
                    Address = requestingPeer.Address,
                    Name = requestingPeer.Name
                }
            });

            foreach (var connectedPeer in connectedPeers.Peers)
            {
                requestingPeer.AddConnectedPeerIfNotExists(connectedPeer.Address, connectedPeer.Name);
            }

            await channel.ShutdownAsync();
        }
    }
}
