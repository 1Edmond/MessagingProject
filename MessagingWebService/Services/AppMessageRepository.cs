using MessagingWebCore.Models;
using MessagingWebService.Interfaces;
using Npgsql;

namespace MessagingWebService.Services;

public class AppMessageRepository : IAppMessageRepository
{
    private readonly string _connectionString;

    public AppMessageRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    public Task AddMessage(AppMessage message)
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
        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task<List<AppMessage>> GetMessages(DateTime? from = null, DateTime? to = null)
    {
        from ??= DateTime.Now.AddHours(-5);
        to ??= DateTime.Now;

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
