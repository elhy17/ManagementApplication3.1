using System;
using System.Collections.Generic;
using System.IO;
using DidiApp.Models;
using Microsoft.Data.Sqlite;
using TaskStatus = DidiApp.Models.TaskStatus;

namespace DidiApp.Services;

public sealed class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService()
    {
        var dataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DidiApp");
        Directory.CreateDirectory(dataRoot);
        var dbPath = Path.Combine(dataRoot, "didi.db");

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath, Mode = SqliteOpenMode.ReadWriteCreate, Cache = SqliteCacheMode.Shared
        };

        _connectionString = builder.ToString();
        Initialize();
    }

    private void Initialize()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA journal_mode=WAL; PRAGMA synchronous=NORMAL;";
            pragma.ExecuteNonQuery();
        }

        using (var createTasks = connection.CreateCommand())
        {
            createTasks.CommandText = @"
            CREATE TABLE IF NOT EXISTS tasks (
                id TEXT PRIMARY KEY,
                title TEXT NOT NULL,
                subject TEXT,
                category TEXT NOT NULL DEFAULT 'Work',
                priority INTEGER NOT NULL,
                status INTEGER NOT NULL,
                due_date TEXT,
                estimated_minutes INTEGER,
                logged_minutes INTEGER NOT NULL,
                is_study INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL,
                paused_minutes INTEGER NOT NULL DEFAULT 0
            );";
            createTasks.ExecuteNonQuery();
        }

        using (var alterCategory = connection.CreateCommand())
        {
            alterCategory.CommandText = "ALTER TABLE tasks ADD COLUMN category TEXT NOT NULL DEFAULT 'Work';";
            try { alterCategory.ExecuteNonQuery(); } catch { }
        }
        
        using (var alterPause = connection.CreateCommand())
        {
            alterPause.CommandText = "ALTER TABLE tasks ADD COLUMN paused_minutes INTEGER NOT NULL DEFAULT 0;";
            try { alterPause.ExecuteNonQuery(); } catch { }
        }

        using (var createNotes = connection.CreateCommand())
        {
            createNotes.CommandText = @"
            CREATE TABLE IF NOT EXISTS notes (
                id TEXT PRIMARY KEY, content TEXT NOT NULL, created_at TEXT NOT NULL, updated_at TEXT NOT NULL
            );";
            createNotes.ExecuteNonQuery();
        }
    }

    public List<TaskItem> LoadTasks()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, title, subject, category, priority, status, due_date, estimated_minutes, logged_minutes, created_at, paused_minutes FROM tasks;";
        using var reader = command.ExecuteReader();

        var results = new List<TaskItem>();
        while (reader.Read())
        {
            results.Add(new TaskItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Subject = reader.IsDBNull(2) ? null : reader.GetString(2),
                Category = reader.GetString(3),
                Priority = (TaskPriority)reader.GetInt32(4),
                Status = (TaskStatus)reader.GetInt32(5),
                DueDate = reader.IsDBNull(6) ? null : DateTime.Parse(reader.GetString(6)),
                EstimatedDuration = reader.IsDBNull(7) ? null : TimeSpan.FromMinutes(reader.GetInt32(7)),
                LoggedDuration = TimeSpan.FromMinutes(reader.GetInt32(8)),
                CreatedAt = DateTime.Parse(reader.GetString(9)),
                PausedDuration = reader.FieldCount > 10 && !reader.IsDBNull(10) ? TimeSpan.FromMinutes(reader.GetInt32(10)) : TimeSpan.Zero
            });
        }
        return results;
    }

    public void UpsertTask(TaskItem task)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        
        command.CommandText = @"
            INSERT INTO tasks (id, title, subject, category, priority, status, due_date, estimated_minutes, logged_minutes, is_study, created_at, paused_minutes)
            VALUES ($id, $title, $subject, $category, $priority, $status, $due_date, $estimated, $logged, 0, $created_at, $paused)
            ON CONFLICT(id) DO UPDATE SET
                title = excluded.title, category = excluded.category, status = excluded.status, 
                logged_minutes = excluded.logged_minutes, paused_minutes = excluded.paused_minutes;";

        command.Parameters.AddWithValue("$id", task.Id.ToString());
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$subject", (object?)task.Subject ?? DBNull.Value);
        command.Parameters.AddWithValue("$category", task.Category);
        command.Parameters.AddWithValue("$priority", (int)task.Priority);
        command.Parameters.AddWithValue("$status", (int)task.Status);
        command.Parameters.AddWithValue("$due_date", task.DueDate?.ToString("O") ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$estimated", task.EstimatedDuration?.TotalMinutes ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$logged", (int)Math.Round(task.LoggedDuration.TotalMinutes));
        command.Parameters.AddWithValue("$created_at", task.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$paused", (int)Math.Round(task.PausedDuration.TotalMinutes));
        command.ExecuteNonQuery();
    }

    public void DeleteTask(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tasks WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());
        command.ExecuteNonQuery();
    }

    public List<NoteItem> LoadNotes()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, content, created_at, updated_at FROM notes ORDER BY updated_at DESC;";
        using var reader = command.ExecuteReader();
        var results = new List<NoteItem>();
        while (reader.Read())
        {
            results.Add(new NoteItem
            {
                Id = Guid.Parse(reader.GetString(0)),
                Content = reader.GetString(1),
                CreatedAt = DateTime.Parse(reader.GetString(2)),
                UpdatedAt = DateTime.Parse(reader.GetString(3))
            });
        }
        return results;
    }

    public void UpsertNote(NoteItem note)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO notes (id, content, created_at, updated_at)
            VALUES ($id, $content, $created_at, $updated_at)
            ON CONFLICT(id) DO UPDATE SET content = excluded.content, updated_at = excluded.updated_at;";
        command.Parameters.AddWithValue("$id", note.Id.ToString());
        command.Parameters.AddWithValue("$content", note.Content);
        command.Parameters.AddWithValue("$created_at", note.CreatedAt.ToString("O"));
        command.Parameters.AddWithValue("$updated_at", note.UpdatedAt.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void DeleteNote(Guid id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM notes WHERE id = $id;";
        command.Parameters.AddWithValue("$id", id.ToString());
        command.ExecuteNonQuery();
    }
}