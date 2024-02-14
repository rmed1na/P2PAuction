using Auction.Data.Models;
using Microsoft.Extensions.Caching.Memory;

namespace Auction.Data.Repositories
{
    public class PeerRepository : IPeerRepository
    {
        private readonly List<Peer> _peers = [];
        private readonly IMemoryCache _cache;
        private readonly string _peersKey = "peers";

        public PeerRepository(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void AddPeer(Peer peer)
        {
            var peers = _cache.Get<List<Peer>>(_peersKey);
            
            peers ??= [];
            peers.Add(peer);
            _cache.Set(_peersKey, peers);
        }

        public List<Peer> GetPeers()
            => _cache.Get<List<Peer>>(_peersKey) ?? [];
    }
}