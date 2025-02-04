using Microsoft.AspNetCore.Mvc;

namespace MessagingWebService.Controllers;

[Route("ws/messages")]
public class AppMessagesSocketController : ControllerBase
{

    public async Task HandleWebSocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
           


        }
    }
}
