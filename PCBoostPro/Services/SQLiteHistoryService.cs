using System.Collections.ObjectModel;
using Microsoft.Data.Sqlite;
using PCBoostPro.Models;

namespace PCBoostPro.Services;

public class SQLiteHistoryService
{
    private readonly string _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PCBoostPro", "history.db");

    public SQLiteHistoryService()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath)!);
        EnsureDatabase();
    }

    private void EnsureDatabase()
    {
        using var connection = new SqliteConnection($"Data Source={_databasePath}");
        connection.Open();

        var commandText = @"
            CREATE TABLE IF NOT EXISTS history (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                timestamp TEXT NOT NULL,
                action TEXT NOT NULL,
                details TEXT NOT NULL
            );";

        using var command = new SqliteCommand(commandText, connection);
        command.ExecuteNonQuery();
    }

    public void Save(string action, string details)
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            using var command = new SqliteCommand(
                "INSERT INTO history (timestamp, action, details) VALUES (@timestamp, @action, @details)",
                connection);

            command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@action", action);
            command.Parameters.AddWithValue("@details", details);
            command.ExecuteNonQuery();
        }
        catch
        {
            // Silently ignore storage errors to avoid crashing the app.
        }
    }

    public ObservableCollection<HistoryEntry> GetRecentHistory(int count = 20)
    {
        var history = new ObservableCollection<HistoryEntry>();

        try
        {
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT timestamp, action, details FROM history ORDER BY id DESC LIMIT @limit",
                connection);
            command.Parameters.AddWithValue("@limit", count);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                history.Add(new HistoryEntry
                {
                    Timestamp = reader.GetString(0),
                    Action = reader.GetString(1),
                    Details = reader.GetString(2)
                });
            }
        }
        catch
        {
            // Silent fallback. UI still loads with empty history.
        }

        return history;
    }
}
