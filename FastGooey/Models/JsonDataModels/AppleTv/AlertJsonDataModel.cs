namespace FastGooey.Models.JsonDataModels.AppleTv;

public class AlertJsonDataModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    
    public List<NavigationButtonJsonDataModel> NavigationButtons { get; set; } = [];
}

