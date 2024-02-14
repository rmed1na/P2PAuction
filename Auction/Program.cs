using Auction.Application.Auction;
using Auction.Application.Peer;
using Auction.Data.Models;
using Auction.Data.Repositories;
using Auction.Services;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
    .AddMemoryCache()
    .AddScoped<IPeerRepository, PeerRepository>()
    .AddScoped<IAuctionRepository, AuctionRepository>()
    .BuildServiceProvider();

#region Dependencies
var repositories = new
{
    peer = serviceProvider.GetRequiredService<IPeerRepository>(),
    auction = serviceProvider.GetRequiredService<IAuctionRepository>()
};
var peerRequestHandler = new PeerRequestHandler(repositories.peer);
#endregion

Console.WriteLine("Welcome to the P2P auction system\n\n");
Console.WriteLine("-------------------------------------");

#region Self server setup
Console.Write("Enter a name for this peer: ");

var peerName = Console.ReadLine() ?? Guid.NewGuid().ToString();

Console.Write($"Enter a port for {peerName}: ");

if (!int.TryParse(Console.ReadLine(), out int peerPort))
{
    Console.WriteLine("Invalid port. Closing...");
    Console.ReadKey();
    return;
}

var peer = new Peer(peerPort, peerName);
var server = new Server
{
    Services = 
    {
        Message.BindService(new MessageService()),
        PeerHandler.BindService(new PeerService(peer)),
        AuctionHandler.BindService(new AuctionService(repositories.auction))
    },
    Ports = { new ServerPort("localhost", peer.Port, ServerCredentials.Insecure) }
};

server.Start();
Console.WriteLine($"Server listening on port: {peer.Port}\n");
#endregion

#region Connection to peers
Console.Write("Provide the port of a fellow peer (optional): ");

if (int.TryParse(Console.ReadLine(), out int fellowPeerPort))
    await peerRequestHandler.GetConnectedPeersAsync(peer, fellowPeerPort);

Console.WriteLine($"Connected peers: {peer.ConnectedPeers.Count}\n");
#endregion
Console.Clear();
var auctionRequestHandler = new AuctionRequestHandler(peer, repositories.auction);
#region Menu
string? command;
do
{
    Console.WriteLine($"\n[Connected peers: {peer.ConnectedPeers.Count}]");
    Console.WriteLine("Choose an action command: ");
    command = Console.ReadLine();

    switch (command)
    {
        case "auction -i":
            Console.Write("\nEnter the item: ");
            var item = Console.ReadLine();

            if (string.IsNullOrEmpty(item))
            {
                Console.Write("Invalid item description/title");
                break;
            }

            Console.Write("Enter the price: ");
            var priceStr = Console.ReadLine();
            
            if (!double.TryParse(priceStr, out double price))
            {
                Console.Write("Invalid price");
                break;
            }

            auctionRequestHandler.Initialize(item, price, peer.Name);
            break;
        case "bid":
            Console.Write("Enter the auction friendly id: ");
            var auctionId = Console.ReadLine();
            var auction = repositories.auction.GetAuction(auctionId ?? string.Empty);
            
            if (auction == null || string.IsNullOrEmpty(auctionId))
            {
                Console.WriteLine("Invalid auction id");
                break;
            }

            Console.Write("Enter the bid price: ");

            if (!double.TryParse(Console.ReadLine(), out double bidPrice))
            {
                Console.WriteLine("Invalid bid price");
                break;
            }

            auctionRequestHandler.SetBid(auctionId, bidPrice, peer.Name);
            break;
        default:
            break;
    }

} while (!string.IsNullOrEmpty(command) && !command.Equals("exit"));

Console.WriteLine("Press any key to exit");
Console.ReadKey();
#endregion