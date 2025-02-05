using MessagingWebCore.Models;

namespace MessagingWebService.Interfaces;

public interface IAppMessageRepository
{
    Task AddMessage(AppMessage message);
    Task<List<AppMessage>> GetMessages(DateTime? from = null, DateTime? to = null);
}
