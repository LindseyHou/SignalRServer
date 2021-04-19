using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRServer.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;

namespace SignalRServer.Controllers
{
    public class MainController : Controller
    {
        private readonly IHubContext<MainHub> _hubContext;
        private readonly ILogger<MainController> _logger;
        private ConnectionFactory factory;
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        public MainController(IHubContext<MainHub> hubContext, ILogger<MainController> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.QueueDeclare(queue: "SendMessage", durable: false, exclusive: false, autoDelete: false, arguments: null);
            consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var lines = message.Split('&');
                var groupName = lines[0];
                var methodName = lines[1];
                var data = lines[2];
                _logger.LogInformation(
                    " [x] Received {0} \t groupName:{1} \t methodName:{2} \t String:{3}",
                    DateTime.Now.ToString("u"),
                    groupName,
                    methodName,
                    data
                );
                if (groupName == "ALL")
                {
                    await hubContext.Clients.All.SendAsync(methodName, data);
                }
                else
                {
                    await hubContext.Clients.Group(groupName).SendAsync(methodName, data);
                }
            };
            channel.BasicConsume(queue: "SendMessage", autoAck: true, consumer: consumer);
        }
    }
}
