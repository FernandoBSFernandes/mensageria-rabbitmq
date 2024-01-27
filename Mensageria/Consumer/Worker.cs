using Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var conexao = connectionFactory.CreateConnection();
            using var channel = conexao.CreateModel();

            channel.QueueDeclare(
                queue: "fila", // Nome da fila
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(channel);

            //Interceptando o evento para realizar uma ação específica.
            consumer.Received += (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();

                var message = Encoding.UTF8.GetString(body);

                var pedido = JsonSerializer.Deserialize<Pedido>(message);

                Console.WriteLine(pedido?.ToString());
            };

            //Após executar o nosso evento, a mensagem, tendo dado tudo certo, é removida da fila através do método BasicConsume
            channel.BasicConsume(queue: "fila", autoAck: true, consumer: consumer);

            await Task.Delay(2000, stoppingToken);
        }
    }
}