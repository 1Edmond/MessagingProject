using MessagingWebCore.Models;

using MessagingWebService.Helpers;
using MessagingWebService.Interfaces;
using Npgsql;

namespace MessagingWebService.Services;

public class AppMessageRepository : IAppMessageRepository
{
    AppMessageWebSocketService appMessageWebSocketService;
    private readonly string _connectionString;
    private readonly ILogger<AppMessageRepository> _logger;
    public AppMessageRepository(IConfiguration configuration, AppMessageWebSocketService appMessageWebSocketService, ILogger<AppMessageRepository> logger)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")!;

        //_connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
        //             $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
        //             $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
        //             $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

        this.appMessageWebSocketService = appMessageWebSocketService;
        _logger.LogInformation("Создание таблицы, если она не существует");
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS messages (\r\n    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\r\n    text VARCHAR NOT NULL,\r\n    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,\r\n    sequence_number INT NOT NULL\r\n);"
           , connection);
        command.ExecuteNonQuery();
        connection.Close();
        _logger.LogInformation("Таблица успешно создана");
    }
    public async Task AddMessage(AppMessage message)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
       
        var command = new NpgsqlCommand("" +
            "INSERT INTO messages (id, text, timestamp, sequence_number) " +
            "VALUES " +
            "(@id, @text, @timestamp, @sequence_number)"
            , connection);
        _logger.LogInformation("Добавление параметров в запрос на вставку");
        command.Parameters.AddWithValue("@id", message.Id);
        command.Parameters.AddWithValue("@text", message.Text);
        command.Parameters.AddWithValue("@timestamp", message.Timestamp);
        command.Parameters.AddWithValue("@sequence_number", message.SequenceNumber);
        _logger.LogInformation("Выполнение запроса");
        var number = command.ExecuteNonQuery();
        if(number > 0)
        {
            _logger.LogInformation("Отправляет данные клиентам, подключенным к сокету");
            await appMessageWebSocketService.SendMessageToClients(message);
        }
    }

    public Task<List<AppMessage>> GetMessages(DateTime? from, DateTime? to)
    {
        from ??= DateTime.Now.AddMinutes( - HistoriqueHelper.UserAccessLastMinutesHistoryTime);
        to ??= DateTime.Now;

        var messages = new List<AppMessage>();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand("SELECT * FROM messages WHERE timestamp BETWEEN @from AND @to", connection);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
        
        _logger.LogInformation($"Получение данных за последние {HistoriqueHelper.UserAccessLastMinutesHistoryTime} минут ");
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            messages.Add(new AppMessage
            {
                Id = reader.GetGuid(0),
                Text = reader.GetString(1),
                Timestamp = reader.GetDateTime(2),
                SequenceNumber = reader.GetInt32(3)
            });
        }


        return Task.FromResult(messages);
    }
}
