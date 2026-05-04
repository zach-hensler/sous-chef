using core.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using services;

namespace sous_chef.Pages;

public record LineChartData {
    public record Point {
        public required string x { get; init; }
        public required string y { get; init; }
    }

    public List<string> Labels { get; init; } = [];
    public List<Point>? DataSet { get; init; }

    public LineChartData(DateOnly start, DateOnly end, List<TotalMonthlyCount>? datapoints) {
        if (datapoints == null) {
            return;
        }
            
        DataSet = datapoints
            .Select(p => new Point {
                x = p.MonthStart.ToString(),
                y = p.Count.ToString()
            })
            .ToList();

        // pad returned dates w/ zeroes
        for (var day = start; day <= end; day = day.AddMonths(1)) {
            Labels.Add(day.ToString());
            if (!DataSet.Exists(d => d.x == day.ToString())) {
                DataSet.Add(new Point {
                    x = day.ToString(),
                    y = "0"
                });
            }
        }

        // re-sort because of newly added zeroes
        DataSet.Sort((a,b) => DateTime.Parse(a.x).CompareTo(DateTime.Parse(b.x)));
    }
}

public class StatsModel : PageModel {
    public LineChartData? LineChart;
    public List<RecipeMonthlyCount> RecipeCounts { get; set; } = [];

    public async Task OnGet() {
        RecipeCounts = await StatService.GetRecentRecipeCounts(2) ?? [];

        var totals = await StatService.GetTotalCountsByMonth() ?? [];
        var end = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        end = end.AddDays(1 - end.Day); // start of month
        var start = end.AddMonths(-6);
        LineChart = new LineChartData(start, end, totals);
    }
}