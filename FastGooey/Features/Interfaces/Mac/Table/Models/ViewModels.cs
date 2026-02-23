using FastGooey.Models;
using FastGooey.Utils;

namespace FastGooey.Features.Interfaces.Mac.Table.Models;

public class MacInterfaceTableFieldEditorPanelViewModel
{
    public Guid WorkspaceId { get; set; }
    public Guid InterfaceId { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string FieldAlias { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
}

public class MacInterfaceTableItemEditorPanelViewModel
{
    public Guid? WorkspaceId { get; set; }
    public Guid? InterfaceId { get; set; }

    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = new();
    public MacTableItemJsonDataModel? Content { get; set; }
}

public class MacInterfaceTableStructureWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}

public class MacInterfaceTableViewModel
{
    public MacInterfaceTableWorkspaceViewModel? Workspace { get; set; }

    public string WorkspaceId()
    {
        return Workspace!.ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return Workspace!.ContentNode!.DocId.ToBase64Url();
    }
}

public class MacInterfaceTableWorkspaceViewModel
{
    public GooeyInterface? ContentNode { get; set; }
    public MacTableJsonDataModel Data { get; set; } = new();

    public string WorkspaceId()
    {
        return ContentNode!.Workspace.PublicId.ToString();
    }

    public string InterfaceId()
    {
        return ContentNode!.DocId.ToBase64Url();
    }
}

public class TableItemOptionRowViewModel
{
    public int? OptionRowCounter { get; set; }
    public string SelectedAlias { get; set; } = string.Empty;
    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = [];
}