using Auction.Data.Models;
using Grpc.Core;

namespace Auction.Services
{
    public class PeerService : PeerHandler.PeerHandlerBase
    {
        private readonly Peer _peer;

        public PeerService(Peer peer)
        {
            _peer = peer;
        }

        public override async Task<GetConnectedPeersResponse> GetConnectedPeers(GetConnectedPeersRequest request, ServerCallContext context)
        {
            _peer.AddConnectedPeerIfNotExists(request.RequestingPeer.Address, request.RequestingPeer.Name);
            var response = new GetConnectedPeersResponse
            {
                Peers =
                {
                    _peer.ConnectedPeers.Select(i => new ConnectedPeer
                    {
                        Address = i.Key,
                        Name = i.Value
                    })
                }
            };

            return response;
        }
    }
}
