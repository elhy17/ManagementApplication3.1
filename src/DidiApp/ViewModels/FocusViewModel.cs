using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using DidiApp.Models;
using DidiApp.Services;

namespace DidiApp.ViewModels;

public sealed class FocusViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private readonly DispatcherTimer _timer;
    private DateTime _sessionStartTime;
    private DateTime? _pauseStartTime;
    private TimeSpan _accumulatedTime;
    private TimeSpan _accumulatedPauseTime;
    private bool _isRunning;
    private bool _goalNotified;
    private string _timeDisplay = "00:00:00";
    private int _targetFocusMinutes = 25;
    
    private string _selectedCategory = "Work";
    private TaskItem? _selectedTask;
    private List<TaskItem> _allTasks = new();

    public FocusViewModel()
    {
        Categories = new ObservableCollection<string> { "Work", "School", "Private Life" };
        AvailableTasks = new ObservableCollection<TaskItem>();
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += (s, e) => UpdateTime();
        
        StartCommand = new RelayCommand(Start, () => !_isRunning && SelectedTask != null);
        PauseCommand = new RelayCommand(Pause, () => _isRunning);
        StopCommand = new RelayCommand(Stop, () => SelectedTask != null && (_isRunning || _accumulatedTime > TimeSpan.Zero || _pauseStartTime != null));
    }

    public ObservableCollection<string> Categories { get; }
    public ObservableCollection<TaskItem> AvailableTasks { get; }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set { SetProperty(ref _selectedCategory, value); FilterTasks(); }
    }

    public TaskItem? SelectedTask 
    { 
        get => _selectedTask; 
        set { SetProperty(ref _selectedTask, value); StartCommand.RaiseCanExecuteChanged(); StopCommand.RaiseCanExecuteChanged(); } 
    }

    public string TimeDisplay { get => _timeDisplay; set => SetProperty(ref _timeDisplay, value); }
    public int TargetFocusMinutes { get => _targetFocusMinutes; set => SetProperty(ref _targetFocusMinutes, value); }

    public RelayCommand StartCommand { get; }
    public RelayCommand PauseCommand { get; }
    public RelayCommand StopCommand { get; }

    public void RefreshTasks()
    {
        _allTasks = _db.LoadTasks().OrderByDescending(t => t.CreatedAt).ToList();
        FilterTasks();
    }

    private void FilterTasks()
    {
        AvailableTasks.Clear();
        foreach (var task in _allTasks.Where(t => t.Category == SelectedCategory)) AvailableTasks.Add(task);
        SelectedTask = AvailableTasks.FirstOrDefault();
    }

    private void Start()
    {
        if (SelectedTask == null) return;
        
        if (_pauseStartTime.HasValue)
        {
            _accumulatedPauseTime += DateTime.UtcNow - _pauseStartTime.Value;
            _pauseStartTime = null;
        }

        _sessionStartTime = DateTime.UtcNow;
        _isRunning = true;
        _goalNotified = false;
        _timer.Start();
        
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
    }

    private void Pause()
    {
        if (!_isRunning) return;
        _timer.Stop();
        _isRunning = false;
        
        var sessionDuration = DateTime.UtcNow - _sessionStartTime;
        _accumulatedTime += sessionDuration;
        
        _pauseStartTime = DateTime.UtcNow;
        
        UpdateTimeDisplay(_accumulatedTime);
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
    }

    private void Stop()
    {
        if (_isRunning)
        {
            _accumulatedTime += DateTime.UtcNow - _sessionStartTime;
            _isRunning = false;
            _timer.Stop();
        }

        if (_pauseStartTime.HasValue)
        {
            _accumulatedPauseTime += DateTime.UtcNow - _pauseStartTime.Value;
            _pauseStartTime = null;
        }
        
        if (SelectedTask != null)
        {
            SelectedTask.LoggedDuration += _accumulatedTime;
            SelectedTask.PausedDuration += _accumulatedPauseTime;
            _db.UpsertTask(SelectedTask);
        }

        _accumulatedTime = TimeSpan.Zero;
        _accumulatedPauseTime = TimeSpan.Zero;
        UpdateTimeDisplay(_accumulatedTime);
        
        StartCommand.RaiseCanExecuteChanged();
        PauseCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
    }

    private void UpdateTime()
    {
        var currentSession = DateTime.UtcNow - _sessionStartTime;
        var totalTime = _accumulatedTime + currentSession;
        UpdateTimeDisplay(totalTime);

        // Notify at goal, but let the clock keep ticking!!!
        if (totalTime.TotalMinutes >= TargetFocusMinutes && !_goalNotified)
        {
            _goalNotified = true;
            NotificationService.SendMacNotification("Goal Reached!", $"You hit {TargetFocusMinutes} minutes. Keep going or take a break!");
        }
    }

    private void UpdateTimeDisplay(TimeSpan ts) => TimeDisplay = ts.ToString(@"hh\:mm\:ss");
}