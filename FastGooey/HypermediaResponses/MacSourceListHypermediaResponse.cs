using FastGooey.Features.Interfaces.Mac.SourceList.Models;

namespace FastGooey.HypermediaResponses;

public class MacSourceListHypermediaResponse : IHypermediaResponse
{
    public Guid InterfaceId { get; set; } = Guid.Empty;
    public string Platform { get; set; } = "Mac";
    public string View { get; set; } = "SourceList";

    public MacSourceListContent Content { get; set; } = new();
}

public class MacSourceListContent
{
    public List<MacSourceListGroupResponse> Groups { get; set; } = [];
}

public class MacSourceListGroupResponse
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string GroupName { get; set; } = string.Empty;
    public List<MacSourceListGroupItemResponse> GroupItems { get; set; } = [];

    public MacSourceListGroupResponse()
    {
    }

    public MacSourceListGroupResponse(MacSourceListGroupJsonDataModel model)
    {
        Identifier = model.Identifier;
        GroupName = model.GroupName;
        GroupItems = model.GroupItems.Select(x => new MacSourceListGroupItemResponse(x)).ToList();
    }
}

public class MacSourceListGroupItemResponse
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;

    public MacSourceListGroupItemResponse()
    {
    }

    public MacSourceListGroupItemResponse(MacSourceListGroupItemJsonDataModel model)
    {
        Identifier = model.Identifier;
        Title = model.Title;
        Icon = model.Icon;
        Url = model.Url;
    }
}
