namespace FastGooey.Features.Widgets.Clock.Models.ViewModels.Clock;

public class ClockPreviewPanelViewModel
{
    public bool PreviewAvailable { get; set; } = false;
    public string Time { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
}