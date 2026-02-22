namespace FastGooey.Features.Interfaces.AppleTv.MediaGrid.Models;

public class AppleTvMediaGridJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    public List<AppleTvMediaGridItemJsonDataModel> MediaItems { get; set; } = [];
}

public class AppleTvMediaGridItemJsonDataModel
{
    public string Guid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string LinkTo { get; set; } = string.Empty;
    public string PreviewMedia { get; set; } = string.Empty;
}