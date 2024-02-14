using Auction;
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
var peerRequestHandler = new PeerRequestHandler();
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
try
{
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
}
catch (IOException ioEx)
{
    _ = ioEx; // Debugging
    Console.WriteLine($"Error starting server. Maybe the port is already in use, try a different one. Message: {ioEx.Message}");
    Console.WriteLine("Press any key to exit");
    Console.ReadKey();
    return;
}
catch (Exception ex)

{
    Console.WriteLine($"Error starting server: {ex.Message}");
}
#endregion

#region Connection to peers
Console.Write("Provide the port of a fellow peer (If none just press enter): ");

if (int.TryParse(Console.ReadLine(), out int fellowPeerPort))
    await peerRequestHandler.PingFellowPeer(peer, fellowPeerPort);

Console.WriteLine($"Connected peers: {peer.ConnectedPeers.Count}\n");
#endregion

Console.Clear();

#region Menu
var menu = new Menu(
    peer,
    repositories.peer,
    repositories.auction);

menu.Start();

Console.WriteLine("Press any key to exit");
Console.ReadKey();
#endregion