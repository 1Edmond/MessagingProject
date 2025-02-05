using MessagingWebService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace MessagingWebService.Controllers;

[Route("ws/messages")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AppMessagesSocketController : ControllerBase
{
    AppMessageWebSocketService appMessageWebSocketService;

    public AppMessagesSocketController(AppMessageWebSocketService appMessageWebSocketService)
    {
        this.appMessageWebSocketService = appMessageWebSocketService;
    }

    public async Task HandleWebSocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await appMessageWebSocketService.AddClient(webSocket);
            await Echo(webSocket);
        }
        
    }

    private static async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                receiveResult.MessageType,
                receiveResult.EndOfMessage,
                CancellationToken.None);

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }
}
