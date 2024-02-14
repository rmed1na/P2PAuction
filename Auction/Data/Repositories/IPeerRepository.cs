using Auction.Data.Models;

namespace Auction.Data.Repositories
{
    public interface IPeerRepository
    {
        public void AddPeer(Peer peer);
        public List<Peer> GetPeers();
    }
}
