using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using DidiApp.Services;

namespace DidiApp.ViewModels;

public class LegendItem
{
    public string Label { get; set; } = string.Empty;
    public IBrush Color { get; set; } = Brushes.Transparent;
    public string ValueText { get; set; } = string.Empty;
}

public sealed class GoalsViewModel : ViewModelBase
{
    private readonly DatabaseService _db = new();
    private IBrush _pieChartBrush = Brushes.Transparent;

    public GoalsViewModel()
    {
        RefreshGoals();
    }

    public ObservableCollection<LegendItem> Legend { get; } = new();

    public IBrush PieChartBrush
    {
        get => _pieChartBrush;
        private set => SetProperty(ref _pieChartBrush, value);
    }

    public void RefreshGoals()
    {
        var tasks = _db.LoadTasks();
        
        double work = tasks.Where(t => t.Category == "Work").Sum(t => t.LoggedDuration.TotalMinutes);
        double school = tasks.Where(t => t.Category == "School").Sum(t => t.LoggedDuration.TotalMinutes);
        double privateLife = tasks.Where(t => t.Category == "Private Life").Sum(t => t.LoggedDuration.TotalMinutes);
        double pause = tasks.Sum(t => t.PausedDuration.TotalMinutes);

        double total = work + school + privateLife + pause;
        if (total == 0) total = 1;

        Legend.Clear();
        var stops = new GradientStops();
        double currentAngle = 0;

        void AddSlice(Color c, double val, string label)
        {
            double pct = val / total;
            if (pct <= 0) return;
            
            stops.Add(new GradientStop(c, currentAngle));
            currentAngle += pct;
            stops.Add(new GradientStop(c, currentAngle));

            Legend.Add(new LegendItem 
            { 
                Label = label, 
                Color = new SolidColorBrush(c), 
                ValueText = $"{Math.Round(val)} min" 
            });
        }

        AddSlice(Color.Parse("#2563EB"), work, "Work Focus");
        AddSlice(Color.Parse("#0D9488"), school, "School Focus");
        AddSlice(Color.Parse("#DB2777"), privateLife, "Private Life Focus");
        AddSlice(Color.Parse("#94A3B8"), pause, "Pause / Rest Time");

        // FIX: Changed .Stops to .GradientStops
        PieChartBrush = new ConicGradientBrush { GradientStops = stops };
    }
}