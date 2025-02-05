using MessagingWebCore.Models;
using MessagingWebService.Interfaces;
using Npgsql;

namespace MessagingWebService.Services;

public class AppMessageRepository : IAppMessageRepository
{
    AppMessageWebSocketService appMessageWebSocketService;
    private readonly string _connectionString;

    public AppMessageRepository(IConfiguration configuration, AppMessageWebSocketService appMessageWebSocketService)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnectionString")!;
        this.appMessageWebSocketService = appMessageWebSocketService;
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand("CREATE TABLE IF NOT EXISTS messages (\r\n    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\r\n    text VARCHAR NOT NULL,\r\n    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,\r\n    sequence_number INT NOT NULL\r\n);"
           , connection);
        command.ExecuteNonQuery();
        connection.Close();


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

        command.Parameters.AddWithValue("@id", message.Id);
        command.Parameters.AddWithValue("@text", message.Text);
        command.Parameters.AddWithValue("@timestamp", message.Timestamp);
        command.Parameters.AddWithValue("@sequence_number", message.SequenceNumber);
        var number = command.ExecuteNonQuery();
        if(number > 0)
        {
            await appMessageWebSocketService.SendMessageToClients(message);
        }
    }

    public Task<List<AppMessage>> GetMessages(DateTime? from, DateTime? to)
    {
        from ??= DateTime.Now.AddHours(-5);
        to ??= DateTime.Now.AddHours(5);

        var messages = new List<AppMessage>();
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        var command = new NpgsqlCommand("SELECT * FROM messages WHERE timestamp BETWEEN @from AND @to", connection);
        command.Parameters.AddWithValue("@from", from);
        command.Parameters.AddWithValue("@to", to);
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
