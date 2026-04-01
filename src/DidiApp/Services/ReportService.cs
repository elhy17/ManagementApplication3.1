using System;
using System.Collections.Generic;
using System.Linq;
using DidiApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

// ✅ Alias to avoid conflict with System.Threading.Tasks.TaskStatus
using ModelTaskStatus = DidiApp.Models.TaskStatus;

namespace DidiApp.Services;

public sealed class ReportService
{
    public void GenerateReport(string filePath, IEnumerable<TaskItem> tasks, string title)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var list = tasks.ToList();

        int Minutes(TaskItem t) => (int)Math.Round(t.LoggedDuration.TotalMinutes);

        var minutesByPriority = Enum.GetValues<TaskPriority>()
            .ToDictionary(p => p, p => list.Where(t => t.Priority == p).Sum(Minutes));
        
        var countByStatus = Enum.GetValues<ModelTaskStatus>()
            .ToDictionary(s => s, s => list.Count(t => t.Status == s));
        
        var today = DateTime.Today;
        var last7 = Enumerable.Range(0, 7)
            .Select(i => today.AddDays(-6 + i))
            .ToList();

        var minutesByDay = last7.ToDictionary(
            d => d,
            d => list.Where(t => t.CreatedAt.Date == d.Date).Sum(Minutes));

        var maxPriority = Math.Max(1, minutesByPriority.Values.Max());
        var maxStatus = Math.Max(1, countByStatus.Values.Max());
        var maxDay = Math.Max(1, minutesByDay.Values.Max());

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);

                page.Header()
                    .Text(title)
                    .FontSize(20)
                    .SemiBold();

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Text($"Generated: {DateTime.Now:g}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Element(e => MetricCard(e, "Total tasks", list.Count.ToString()));
                        row.RelativeItem().Element(e => MetricCard(e, "Total minutes", list.Sum(Minutes).ToString()));
                        row.RelativeItem().Element(e => MetricCard(
                            e,
                            "Archived",
                            list.Count(t => t.Status == ModelTaskStatus.Archived).ToString()
                        ));
                    });

                    col.Item().Text("Minutes by priority").SemiBold().FontSize(14);
                    col.Item().Element(e => BarChart(e,
                        minutesByPriority.Select(kv =>
                            (kv.Key.ToString(), kv.Value, kv.Value / (float)maxPriority)
                        ).ToList(),
                        valueSuffix: " min"));

                    col.Item().Text("Tasks by status").SemiBold().FontSize(14);
                    col.Item().Element(e => BarChart(e,
                        countByStatus.Select(kv =>
                            (kv.Key.ToString(), kv.Value, kv.Value / (float)maxStatus)
                        ).ToList(),
                        valueSuffix: ""));

                    col.Item().Text("Activity trend (last 7 days)").SemiBold().FontSize(14);
                    col.Item().Element(e => SparklineBars(e,
                        last7.Select(d =>
                            (d.ToString("ddd"), minutesByDay[d], minutesByDay[d] / (float)maxDay)
                        ).ToList()));

                    col.Item().Text("Tasks").SemiBold().FontSize(14);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.ConstantColumn(90);
                            cols.ConstantColumn(110);
                            cols.ConstantColumn(90);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellHeader).Text("Title");
                            h.Cell().Element(CellHeader).Text("Priority");
                            h.Cell().Element(CellHeader).Text("Status");
                            h.Cell().Element(CellHeader).AlignRight().Text("Minutes");
                        });

                        foreach (var t in list.OrderByDescending(x => x.CreatedAt))
                        {
                            table.Cell().Element(CellBody).Text(t.Title);
                            table.Cell().Element(CellBody).Text(t.Priority.ToString());
                            table.Cell().Element(CellBody).Text(t.Status.ToString());
                            table.Cell().Element(CellBody).AlignRight().Text(Minutes(t).ToString());
                        }
                    });
                });

                page.Footer().AlignRight()
                    .Text("Didi Workspace Report")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken2);
            });
        }).GeneratePdf(filePath);

        static IContainer CellHeader(IContainer c) =>
            c.Background(Colors.Grey.Lighten3).Padding(6).DefaultTextStyle(x => x.SemiBold());

        static IContainer CellBody(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(6);

        static void MetricCard(IContainer container, string label, string value)
        {
            container
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Column(c =>
                {
                    c.Item().Text(label).FontSize(10).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(value).FontSize(16).SemiBold();
                });
        }

        static void BarChart(IContainer container, List<(string label, int value, float pct)> items, string valueSuffix)
        {
            container.Column(c =>
            {
                c.Spacing(6);

                foreach (var (label, value, pct) in items)
                {
                    c.Item().Row(r =>
                    {
                        r.ConstantItem(140).Text(label).FontSize(10);
                        r.RelativeItem().Height(12).Background(Colors.Grey.Lighten3)
                            .AlignLeft()
                            .Element(e => e
                                .Width(Math.Max(2, pct * 300))
                                .Background(Colors.Blue.Medium));

                        r.ConstantItem(70).AlignRight().Text($"{value}{valueSuffix}").FontSize(10);
                    });
                }
            });
        }

        static void SparklineBars(IContainer container, List<(string label, int value, float pct)> items)
        {
            container.Row(r =>
            {
                foreach (var (label, value, pct) in items)
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().AlignCenter().Text(label).FontSize(9).FontColor(Colors.Grey.Darken1);
                        c.Item().Height(55).AlignBottom()
                            .Element(e => e
                                .Height(Math.Max(2, pct * 55))
                                .Background(Colors.Green.Medium));
                        c.Item().AlignCenter().Text(value.ToString()).FontSize(9);
                    });
                }
            });
        }
    }
}
