using Grpc.Core;

namespace Auction.Services
{
    public class MessageService : Message.MessageBase
    {
        public override async Task<MessageBody> SendMessage(MessageBody request, ServerCallContext context)
        {
            var message = $"{request.Sender}: {request.Content}";

            Console.WriteLine(message);
            return new MessageBody
            {
                Content = message
            };
        }
    }
}
