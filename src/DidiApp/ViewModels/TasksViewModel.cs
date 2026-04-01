using System;
using System.Collections.ObjectModel;
using System.Linq;
using DidiApp.Models;
using DidiApp.Services;

namespace DidiApp.ViewModels;

public sealed class TasksViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private TaskItem? _selectedTask;
    private string _newTaskTitle = string.Empty;
    private string _selectedCategory = "Work";
    private string _viewFilter = "All Tasks";

    public TasksViewModel()
    {
        Categories = new ObservableCollection<string> { "Work", "School", "Private Life" };
        ViewFilters = new ObservableCollection<string> { "All Tasks", "Work", "School", "Private Life" };
        
        LoadTasks();
        AddTaskCommand = new RelayCommand(AddTask, () => !string.IsNullOrWhiteSpace(NewTaskTitle));
        DeleteTaskCommand = new RelayCommand(DeleteTask, () => SelectedTask != null);
    }

    public ObservableCollection<TaskItem> AllTasks { get; } = new();
    public ObservableCollection<TaskItem> DisplayedTasks { get; } = new();
    
    public ObservableCollection<string> Categories { get; }
    public ObservableCollection<string> ViewFilters { get; }

    public string ViewFilter
    {
        get => _viewFilter;
        set { SetProperty(ref _viewFilter, value); ApplyFilter(); }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set => SetProperty(ref _selectedCategory, value);
    }

    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set { SetProperty(ref _newTaskTitle, value); AddTaskCommand.RaiseCanExecuteChanged(); }
    }

    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set { SetProperty(ref _selectedTask, value); DeleteTaskCommand.RaiseCanExecuteChanged(); }
    }

    public RelayCommand AddTaskCommand { get; }
    public RelayCommand DeleteTaskCommand { get; }

    private void LoadTasks()
    {
        AllTasks.Clear();
        foreach (var t in _db.LoadTasks().OrderByDescending(x => x.CreatedAt)) AllTasks.Add(t);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        DisplayedTasks.Clear();
        var filtered = ViewFilter == "All Tasks" ? AllTasks : AllTasks.Where(t => t.Category == ViewFilter);
        foreach (var task in filtered) DisplayedTasks.Add(task);
    }

    private void AddTask()
    {
        try 
        {
            var task = new TaskItem
            {
                Title = NewTaskTitle?.Trim() ?? "New Task",
                Category = string.IsNullOrWhiteSpace(SelectedCategory) ? "Work" : SelectedCategory,
                CreatedAt = DateTime.UtcNow,
                LoggedDuration = TimeSpan.Zero,
                PausedDuration = TimeSpan.Zero
            };
            _db.UpsertTask(task);
            AllTasks.Insert(0, task);
            ApplyFilter();
            NewTaskTitle = string.Empty;
        }
        catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    private void DeleteTask()
    {
        if (SelectedTask == null) return;
        _db.DeleteTask(SelectedTask.Id);
        AllTasks.Remove(SelectedTask);
        ApplyFilter();
    }
}