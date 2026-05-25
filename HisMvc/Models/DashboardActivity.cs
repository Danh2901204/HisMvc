namespace HisMvc.Models;

/// <summary>
/// Mot dong hoat dong tren dashboard (dung chung moi Area).
/// </summary>
public class DashboardActivity
{
    public DateTime At { get; set; }
    public string Icon { get; set; } = "bi-info-circle";
    public string Title { get; set; } = "";
    public string Detail { get; set; } = "";
    public string Url { get; set; } = "#";
    public string Tag { get; set; } = "";
    public string Priority { get; set; } = "";
}
