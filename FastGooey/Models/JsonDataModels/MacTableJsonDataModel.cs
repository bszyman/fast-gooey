using System.Text.Json.Nodes;

namespace FastGooey.Models.JsonDataModels;

public class MacTableJsonDataModel
{
    public List<MacTableItemJsonDataModel> Data { get; set; } = [];
    public List<MacTableStructureItemJsonDataModel> Structure { get; set; } = [];
    public List<MacTableStructureHeaderJsonDataModel> Header { get; set; } = [];
}

public class MacTableItemJsonDataModel
{
    public Guid Identifier { get; set; } = Guid.Empty;
    public string GooeyName { get; set; } = string.Empty;
    public Dictionary<string, object> Content { get; set; } = new();
}

public class MacTableStructureItemJsonDataModel 
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldAlias { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public List<string> DropdownOptions = new();
}

public class MacTableStructureHeaderJsonDataModel
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldAlias { get; set; } = string.Empty;
}