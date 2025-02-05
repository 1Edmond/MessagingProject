using MessagingWebCore.Models;
using MessagingWebService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MessagingWebService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppMessageController : ControllerBase
{
    private readonly IAppMessageRepository appMessageRepository;
    private readonly ILogger<AppMessageController> _logger;
    public AppMessageController(IAppMessageRepository appMessageRepository, ILogger<AppMessageController> logger)
    {
        this.appMessageRepository = appMessageRepository;
        _logger = logger;
    }



    [HttpPost("add")]
    public async Task<IActionResult> SendMessage(AppMessage message)
    {
        try
        {
            _logger.LogInformation("Генерация информации на стороне сервера для сообщения");
            message.Id = Guid.NewGuid();
            message.Timestamp = DateTime.UtcNow;
            await appMessageRepository.AddMessage(message);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка {ex.Message}");
            return BadRequest(ex.Message);
        }
       
    }

    [HttpGet("")]
    public async Task<IActionResult> GetMessages(DateTime? from, DateTime? to)
    {
        try
        {
            _logger.LogInformation($"Начните восстановление данных с {from.ToString()} до {to.ToString()}");
            var messages = await appMessageRepository.GetMessages(from, to);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка {ex.Message}");
            return BadRequest(ex.Message);
        }
     
    }
}
