using System.Data.SQLite;
using VoiceProcessor.Utilities;

namespace VoiceProcessorServer;

// Stores anonymous data to analyze trends and potential issues
// My first attempt at using SQL and I hate it
public class Telemetry
{
    private readonly string _connectionString;
    private readonly string _filePath = WorkingFolderUtils.GetTelemetryFilePath();

    public Telemetry()
    {
        _connectionString = $"Data Source={_filePath};Version=3;";
    }
    
    // Prevent database weirdness
    private static readonly HashSet<string> AllowedTables =
    [
        "Requests",
        "Errors",
        "ServerStarts"
    ];

    public void LogReceivedRequest(VoiceLineRequest request)
    {
        LogGenericData("Requests", "Received request: " + request);
    }

    public void LogError(string error)
    {
        LogGenericData("Errors", error);
    }
    
    public void LogProcessedRequest(VoiceLineRequest request)
    {
        LogGenericData("Requests", "Processing request with ID " + request.JobId);
    }
    
    public void LogRequestCompletionStatus(VoiceLineRequest request, int characters, double duration, bool success)
    {
        var status = success ? "success" : "failed";
        LogCompletionData($"Request completed with status '{status}' (id: {request.JobId})", characters, duration, status);
    }
    
    public void LogServerStart()
    {
        LogGenericData("ServerStarts", "Server started!");
    }

    private void LogGenericData(string table, string value)
    {
        if (!AllowedTables.Contains(table))
            throw new ArgumentException($"Invalid table name: {table}");

        // Connect to the database
        using var con = new SQLiteConnection(_connectionString);
        con.Open();
        
        // Ensure it exists
        var createSql = $@"
        CREATE TABLE IF NOT EXISTS {table} (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Timestamp TEXT NOT NULL,
            Message TEXT NOT NULL
        );";

        using var ensureCmd = new SQLiteCommand(createSql, con);
        ensureCmd.ExecuteNonQuery();

        // Insert data
        var insertSql = $@"
        INSERT INTO {table} (Timestamp, Message)
        VALUES (@ts, @msg);";

        using var insertCmd = new SQLiteCommand(insertSql, con);
        insertCmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
        insertCmd.Parameters.AddWithValue("@msg", value);
        insertCmd.ExecuteNonQuery();
    }
    
    private void LogCompletionData(string value, int characters, double duration, string status)
    {
        using var con = new SQLiteConnection(_connectionString);
        con.Open();
        
        // Ensure it exists
        var createSql = @"
        CREATE TABLE IF NOT EXISTS GeneratedFiles (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Timestamp TEXT NOT NULL,
            Status TEXT NOT NULL,
            Characters INTEGER NOT NULL,
            Duration REAL NOT NULL,
            Message TEXT NOT NULL
        );";

        using var ensureCmd = new SQLiteCommand(createSql, con);
        ensureCmd.ExecuteNonQuery();

        // Insert data
        var insertSql = @"
        INSERT INTO GeneratedFiles (Timestamp, Status, Characters, Duration, Message)
        VALUES (@ts, @status, @characters, @duration, @msg);";

        using var insertCmd = new SQLiteCommand(insertSql, con);
        insertCmd.Parameters.AddWithValue("@ts", DateTime.UtcNow.ToString("o"));
        insertCmd.Parameters.AddWithValue("@status", status);
        insertCmd.Parameters.AddWithValue("@characters", characters);
        insertCmd.Parameters.AddWithValue("@duration", duration);
        insertCmd.Parameters.AddWithValue("@msg", value);
        insertCmd.ExecuteNonQuery();
    }
}