namespace FastGooey.Features.Interfaces.AppleTv.Shared.Models.JsonDataModels.AppleTv;

public class DescriptiveAlertJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    public string CancelButtonText { get; set; } = string.Empty;
    public string ConfirmButtonText { get; set; } = string.Empty;
    public List<DescriptiveAlertContentNodeJsonDataModel> DescriptiveContent { get; set; } = [];
}

public class DescriptiveAlertContentNodeJsonDataModel
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
