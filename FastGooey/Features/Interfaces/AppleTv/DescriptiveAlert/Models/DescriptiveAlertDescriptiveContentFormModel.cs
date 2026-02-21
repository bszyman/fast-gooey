namespace FastGooey.Features.Interfaces.AppleTv.DescriptiveAlert.Models;

public class DescriptiveAlertDescriptiveContentFormModel
{
    public List<DescriptiveAlertDescriptiveContentNodeFormModel> DescriptiveContent { get; set; } = [];
}

public class DescriptiveAlertDescriptiveContentNodeFormModel
{
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
