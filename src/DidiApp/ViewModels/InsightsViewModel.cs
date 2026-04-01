using System;
using System.Collections.ObjectModel;
using System.Linq;
using DidiApp.Services;

namespace DidiApp.ViewModels;

public class ChartItem
{
    public string Category { get; set; } = string.Empty;
    public double TotalMinutes { get; set; }
    public double MaxValue { get; set; }
    public string DisplayText => $"{Math.Round(TotalMinutes)} min";
}

public sealed class InsightsViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    public ObservableCollection<ChartItem> CategoryData { get; } = new();

    public void RefreshChart()
    {
        CategoryData.Clear();
        var tasks = _db.LoadTasks();
        
        var grouped = tasks.GroupBy(t => t.Category)
                           .Select(g => new { Category = g.Key, Minutes = g.Sum(t => t.LoggedDuration.TotalMinutes) })
                           .ToList();

        var maxMinutes = grouped.Any() ? grouped.Max(g => g.Minutes) : 1;
        if (maxMinutes == 0) maxMinutes = 1;

        foreach (var item in grouped.OrderByDescending(g => g.Minutes))
        {
            CategoryData.Add(new ChartItem 
            { 
                Category = item.Category, 
                TotalMinutes = item.Minutes, 
                MaxValue = maxMinutes 
            });
        }
    }
}
