using MessagingWebCore.Models;
using MessagingWebService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessagingWebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppMessageController : ControllerBase
{
    private readonly IAppMessageRepository appMessageRepository;

    public AppMessageController(IAppMessageRepository appMessageRepository)
    {
        this.appMessageRepository = appMessageRepository;
    }



    [HttpPost("add")]
    public async Task<IActionResult> SendMessage(AppMessage message)
    {
        message.Id = Guid.NewGuid();
        message.Timestamp = DateTime.UtcNow;
        await appMessageRepository.AddMessage(message);
        return Ok();
    }

    [HttpGet("")]
    public async Task<IActionResult> GetMessages(DateTime? from, DateTime? to)
    {
        var messages = await appMessageRepository.GetMessages(from, to);
        return Ok(messages);
    }
}
