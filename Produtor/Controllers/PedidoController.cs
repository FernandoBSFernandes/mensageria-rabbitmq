using System.Text;
using System.Text.Json;
using Core;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace Produtor.Controllers;

[ApiController]
[Route("pedido")]
public class PedidoController : ControllerBase
{

    [HttpPost("criar")]
    public IActionResult Criar()
    {

        //Criando uma conexão com o RabbitMQ, passando as informações necessárias.
        var connectionFactory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        using var conexao = connectionFactory.CreateConnection();
        using var channel = conexao.CreateModel();

        //Definindo detalhes da fila que receberá a mensagem pelo exchange.
        channel.QueueDeclare(
            queue: "fila", // Nome da fila
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var pedido = new Pedido(1, new Usuario(1, "Carlos Magalhães", "carlos@email.com"));
        
        string message = JsonSerializer.Serialize(pedido);
        
        var body = Encoding.UTF8.GetBytes(message);

        // Após preparar a mensagem, chama o método para publicar a mensagem na fila
        channel.BasicPublish(
            exchange: "", // Se vazio, exchange será direct
            routingKey: "fila", 
            basicProperties: null,
            body: body);

        return Ok();
    }


}
