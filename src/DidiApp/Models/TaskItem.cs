using System;
using DidiApp.ViewModels;

namespace DidiApp.Models;

public enum TaskPriority { Low, Medium, High }
public enum TaskStatus { Planned, InProgress, Paused, Done, Archived }

public sealed class TaskItem : ViewModelBase
{
    private Guid _id = Guid.NewGuid();
    private string _title = string.Empty;
    private string? _subject;
    private string _category = "Work";
    private TaskPriority _priority = TaskPriority.Medium;
    private TaskStatus _status = TaskStatus.Planned;
    private DateTime? _dueDate;
    private TimeSpan? _estimatedDuration;
    private TimeSpan _loggedDuration;
    private TimeSpan _pausedDuration;
    private DateTime _createdAt = DateTime.UtcNow;

    public Guid Id { get => _id; set => SetProperty(ref _id, value); }
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string? Subject { get => _subject; set => SetProperty(ref _subject, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }
    public TaskPriority Priority { get => _priority; set => SetProperty(ref _priority, value); }
    public TaskStatus Status { get => _status; set => SetProperty(ref _status, value); }
    public DateTime? DueDate { get => _dueDate; set => SetProperty(ref _dueDate, value); }
    public TimeSpan? EstimatedDuration { get => _estimatedDuration; set => SetProperty(ref _estimatedDuration, value); }
    public TimeSpan LoggedDuration { get => _loggedDuration; set => SetProperty(ref _loggedDuration, value); }
    public TimeSpan PausedDuration { get => _pausedDuration; set => SetProperty(ref _pausedDuration, value); }
    public DateTime CreatedAt { get => _createdAt; set => SetProperty(ref _createdAt, value); }
}