using MessagingWebService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;

namespace MessagingWebService.Controllers;

[Route("ws/messages")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AppMessagesSocketController : ControllerBase
{
    AppMessageWebSocketService appMessageWebSocketService;
    private readonly ILogger<AppMessagesSocketController> _logger;
    public AppMessagesSocketController(AppMessageWebSocketService appMessageWebSocketService, ILogger<AppMessagesSocketController> logger)
    {
        this.appMessageWebSocketService = appMessageWebSocketService;
        _logger = logger;
    }

    public async Task HandleWebSocket()
    {
        _logger.LogInformation("Обработка запроса");
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _logger.LogInformation("Добавить клиента");
            await appMessageWebSocketService.AddClient(webSocket);

            await Echo(webSocket);
        }
        
    }

    private async Task Echo(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        _logger.LogInformation("Настройка ответа сервера, ");

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
        _logger.LogInformation("Закрытие сеанса на сервере");
        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
        _logger.LogInformation("Удаление клиента.");
        await appMessageWebSocketService.RemoveClient(webSocket);
    }
}
