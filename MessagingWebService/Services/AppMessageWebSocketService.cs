using MessagingWebCore.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MessagingWebService.Services;

public class AppMessageWebSocketService
{
    private static List<WebSocket> WebSockets = new List<WebSocket>();
    private readonly ILogger<AppMessageWebSocketService> _logger;

    public AppMessageWebSocketService(ILogger<AppMessageWebSocketService> logger)
    {
        _logger = logger;
    }

    public Task AddClient(WebSocket webSocket)
    {
        WebSockets.Add(webSocket);
        _logger.LogInformation("Клиент успешно добавлен.");
        return Task.CompletedTask;
    }

    public async Task SendMessageToClients(AppMessage message)
    {
        _logger.LogInformation("Сериализация данных для отправки клиенту");
        var jsonAppMessage = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(jsonAppMessage);
        
        var segment = new ArraySegment<byte>(buffer);

        foreach (var socket in WebSockets)
            if (socket.State == WebSocketState.Open)
            {
                _logger.LogInformation("Отправка данных клиенту");
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);   
            }
    }
}
