using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _exchange;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _exchange = _configuration["RabbitMQ:Exchange"];

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"])
            };

            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(exchange: _exchange, type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("--> Connected to RabbitMQ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        public void PublishNewPlatform(PlatformPublishDto platformPublishDto)
        {
            var platformMessage = JsonSerializer.Serialize(platformPublishDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                SendMessage(platformMessage);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Connection Closed...");
            }
        }

        private void SendMessage(string message)
        {
            var messageBody = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: _exchange,
                                routingKey: string.Empty,
                                basicProperties: null,
                                messageBody);

            Console.WriteLine($"--> We have sent {message}");
        }

        public void Dispose()
        {
            Console.WriteLine("--> RabbitMQ Bus Disposed");
            if (_channel.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs args)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
        }
    }
}
