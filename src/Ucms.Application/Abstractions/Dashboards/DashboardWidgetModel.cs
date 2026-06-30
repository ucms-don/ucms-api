namespace Ucms.Application.Abstractions.Dashboards;

public class DashboardWidgetModel
{
    public string? Title { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleKa { get; set; }
    public List<DashboardWidgetItemModel> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public class DashboardWidgetItemModel
{
    public string? Title { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }
    public string? TitleKa { get; set; }
    public int Count { get; set; }
}
