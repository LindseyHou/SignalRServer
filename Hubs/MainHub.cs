using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.AspNetCore.SignalR;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalRServer.Hubs
{
    public class MainHub : Hub
    {
        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;
        public MainHub()
        {
            factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: "ReceiveMessage",
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);
        }
        public override async Task OnConnectedAsync()
        {
            await Clients.All.SendAsync("OnConnectedAsync", "Hello world");
        }
        public async Task SendMessage(string methodName)
        {
            var body = Encoding.UTF8.GetBytes(methodName);
            channel.BasicPublish(exchange: "",
                                routingKey: "ReceiveMessage",
                                basicProperties: null,
                                body: body);
            Debug.WriteLine(" [x] Sent \t methodName: {0}", methodName);
            await Clients.All.SendAsync("ReceiveMessage", $"OK {methodName}");
        }
    }
}
