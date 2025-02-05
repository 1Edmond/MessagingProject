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
        }
        
    }
}
