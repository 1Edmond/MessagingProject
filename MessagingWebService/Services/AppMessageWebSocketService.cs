using MessagingWebCore.Models;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MessagingWebService.Services;

public class AppMessageWebSocketService
{
    private static List<WebSocket> WebSockets = new List<WebSocket>();

    public Task AddClient(WebSocket webSocket)
    {
        WebSockets.Add(webSocket);

        return Task.CompletedTask;
    }

    public async Task SendMessageToClients(AppMessage message)
    {
        
        var jsonAppMessage = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(jsonAppMessage);
        
        var segment = new ArraySegment<byte>(buffer);

        foreach (var socket in WebSockets)
            if (socket.State == WebSocketState.Open)
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None);   
    }
}
