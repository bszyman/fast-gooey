using System.Text.Json;
using System.Text.Json.Nodes;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.FormModels;
using FastGooey.Models.JsonDataModels.Mac;
using FastGooey.Models.ViewModels.Mac;
using FastGooey.Services;
using FastGooey.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/interfaces/mac/table")]
public class MacTableController(
    ILogger<MacTableController> logger,
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext) :
    BaseInterfaceController(keyValueService, dbContext)
{
    private async Task<MacInterfaceTableWorkspaceViewModel> WorkspaceViewModelForInterfaceId(Guid interfaceId)
    {
        return await GetInterfaceViewModelAsync<MacInterfaceTableWorkspaceViewModel, MacTableJsonDataModel>(interfaceId);
    }

    private async Task<MacInterfaceTableStructureWorkspaceViewModel> WorkspaceStructureViewModelForInterfaceId(
        Guid interfaceId)
    {
        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceId));

        var viewModel = new MacInterfaceTableStructureWorkspaceViewModel
        {
            ContentNode = contentNode,
            Data = contentNode.Config.Deserialize<MacTableJsonDataModel>()
        };

        return viewModel;
    }

    [HttpGet("{interfaceId}")]
    public async Task<IActionResult> Index(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var workspaceViewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);
        var viewModel = new MacInterfaceTableViewModel
        {
            Workspace = workspaceViewModel
        };

        return View(viewModel);
    }

    [HttpGet("workspace/{interfaceId}")]
    public async Task<IActionResult> TableWorkspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/MacTable/Workspace.cshtml", viewModel);
    }

    [HttpPost("create-interface")]
    public async Task<IActionResult> CreateInterface()
    {
        if (await InterfaceLimitReachedAsync())
        {
            return Forbid();
        }

        var workspace = GetWorkspace();
        var data = new MacTableJsonDataModel();

        var contentNode = new GooeyInterface
        {
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Platform = "Mac",
            ViewType = "Table",
            Name = "New Table Interface",
            Config = JsonSerializer.SerializeToDocument(data)
        };

        await dbContext.GooeyInterfaces.AddAsync(contentNode);
        await dbContext.SaveChangesAsync();

        var viewModel = new MacInterfaceTableViewModel
        {
            Workspace = new MacInterfaceTableWorkspaceViewModel
            {
                ContentNode = contentNode,
                Data = data
            }
        };

        Response.Headers.Append("HX-Trigger", "refreshInterfaces");

        return PartialView("~/Views/MacTable/Index.cshtml", viewModel);
    }

    [HttpGet("workspace/{interfaceId}/structure")]
    public async Task<IActionResult> TableStructureWorkspace(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var viewModel = await WorkspaceStructureViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/MacTable/StructureWorkspace.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/header-option-row")]
    public async Task<IActionResult> TableHeaderOptionRow(string interfaceId, [FromQuery] int? nextHeaderCounter)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        var viewModel = new TableItemOptionRowViewModel
        {
            OptionRowCounter = nextHeaderCounter + 1,
            Structure = data.Structure
        };

        return PartialView("~/Views/MacTable/Partials/TableHeaderOptionRow.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/save-structure-workspace")]
    public async Task<IActionResult> SaveStructureWorkspace(string interfaceId, [FromForm] MacTableStructureWorkspaceFormModel formModel)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        data.Header = formModel.Headers.Select(x => new MacTableStructureHeaderJsonDataModel
        {
            FieldAlias = x,
            FieldName = data.Structure.FirstOrDefault(y => y.FieldAlias.Equals(x)).FieldName
        }).ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        var viewModel = await WorkspaceStructureViewModelForInterfaceId(interfaceGuid);

        return PartialView("~/Views/MacTable/StructureWorkspace.cshtml", viewModel);
    }

    [HttpGet("{interfaceId}/field-editor-panel/{fieldAlias?}")]
    public async Task<IActionResult> TableFieldEditorPanel(string interfaceId, string? fieldAlias)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        var fieldConfig = string.IsNullOrWhiteSpace(fieldAlias)
            ? new MacTableStructureItemJsonDataModel()
            : data.Structure.FirstOrDefault(x => x.FieldAlias.Equals(fieldAlias));

        var viewModel = new MacInterfaceTableFieldEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            FieldName = fieldConfig.FieldName,
            FieldAlias = fieldConfig.FieldAlias,
            FieldType = fieldConfig.FieldType
        };

        return PartialView("~/Views/MacTable/Partials/TableFieldEditorPanel.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/field-editor-panel/item/{fieldAlias?}")]
    public async Task<IActionResult> SaveTableFieldEditorPanel(string interfaceId, string? fieldAlias, [FromForm] MacTableFieldConfigPanelFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        if (ModelState.IsValid)
        {
            var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();
            var existingFieldType = data.Structure
                .FirstOrDefault(x => x.FieldAlias.Equals(fieldAlias));

            if (string.IsNullOrWhiteSpace(fieldAlias) || existingFieldType is null)
            {
                var fieldType = new MacTableStructureItemJsonDataModel
                {
                    FieldName = form.FieldName,
                    FieldAlias = form.FieldAlias,
                    FieldType = form.FieldType,
                    DropdownOptions = form.DropdownOptions
                };

                data.Structure.Add(fieldType);
            }
            else
            {
                existingFieldType.FieldName = form.FieldName;
                existingFieldType.FieldType = form.FieldType;
                existingFieldType.FieldAlias = form.FieldAlias;
                existingFieldType.DropdownOptions = form.DropdownOptions;
            }

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshStructureWorkspace, toggleEditor");
        }

        var viewModel = new MacInterfaceTableFieldEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            FieldName = form.FieldName,
            FieldAlias = form.FieldAlias,
            FieldType = form.FieldType
        };

        return PartialView("~/Views/MacTable/Partials/TableFieldEditorPanel.cshtml", viewModel);
    }

    [HttpDelete("{interfaceId}/field-editor-panel/item/{fieldAlias}")]
    public async Task<IActionResult> DeleteFieldType(string interfaceId, string fieldAlias)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();
        var item = data.Structure
            .FirstOrDefault(x => x.FieldAlias.Equals(fieldAlias));

        if (item is null)
        {
            return NotFound();
        }

        data.Structure = data.Structure
            .Where(x => !x.FieldAlias.Equals(fieldAlias))
            .ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshStructureWorkspace, toggleEditor");

        return Ok();
    }

    [HttpGet("{interfaceId}/field-editor-panel/item/new-dropdown")]
    public IActionResult TableFieldEditorNewDropdownPanel(string interfaceId)
    {
        if (!GuidShortId.TryParse(interfaceId, out _))
        {
            return NotFound();
        }

        return PartialView("~/Views/MacTable/Partials/TableItemOptionRow.cshtml", "");
    }

    [HttpGet("{interfaceId}/item-editor-panel/{itemId:guid?}")]
    public async Task<IActionResult> TableItemEditorPanel(string interfaceId, Guid? itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        var content = itemId.HasValue
            ? data?.Data.FirstOrDefault(x => x.Identifier.Equals(itemId.Value))
            : new MacTableItemJsonDataModel();

        var viewModel = new MacInterfaceTableItemEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Structure = data.Structure,
            Content = content
        };

        return PartialView("~/Views/MacTable/Partials/TableItemEditorPanel.cshtml", viewModel);
    }

    [HttpPost("{interfaceId}/item-editor-panel/{itemId:guid?}")]
    public async Task<IActionResult> SaveTableItemEditorPanel(
        string interfaceId,
        Guid? itemId,
        [FromForm] MacTableItemEditorPanelFormModel form)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        var tableItem = data?.Data
                            .FirstOrDefault(x => x.Identifier == itemId.GetValueOrDefault())
                        ?? new MacTableItemJsonDataModel { Identifier = Guid.NewGuid() };

        if (!ModelState.IsValid)
        {
            Response.Headers.Append("HX-Retarget", "#editorPanel");
        }
        else
        {
            if (!string.IsNullOrEmpty(form.GooeyName))
            {
                tableItem.GooeyName = form.GooeyName;
            }

            if (!string.IsNullOrEmpty(form.RelatedUrl))
            {
                tableItem.RelatedUrl = form.RelatedUrl!;
            }

            if (!string.IsNullOrEmpty(form.DoubleClickUrl))
            {
                tableItem.DoubleClickUrl = form.DoubleClickUrl!;
            }

            foreach (var field in data.Structure)
            {
                var rawValues = Request.Form[field.FieldAlias];
                if (rawValues.Any())
                {
                    var stringValue = rawValues.First();

                    switch (field.FieldType)
                    {
                        case "textString":
                        case "longText":
                        case "url":
                        case "email":
                        case "phone":
                        case "dropdown":
                            UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(stringValue));
                            break;
                        case "integer":
                            var intValue = int.TryParse(stringValue, out var i) ? (object)i : null;
                            if (intValue is not null)
                                UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(intValue));
                            break;
                        case "boolean":
                            var boolValue = bool.TryParse(stringValue, out var b) ? (object)b : null;
                            if (boolValue is not null)
                                UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(boolValue));
                            break;
                        case "date":
                            var dateValue = DateTime.TryParse(stringValue, out var d) ? (object)d.Date : null;
                            if (dateValue is not null)
                                UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(dateValue));
                            break;
                        case "time":
                            var timeValue = TimeSpan.TryParse(stringValue, out var t) ? (object)t : null;
                            if (timeValue is not null)
                                UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(timeValue));
                            break;
                        case "dateTime":
                            var dateTimeValue = DateTime.TryParse(stringValue, out var dt) ? (object)dt : null;
                            if (dateTimeValue is not null)
                                UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(dateTimeValue));
                            break;
                        default:
                            UpdateKeyOrAddIfNotExists(tableItem?.Content, field.FieldAlias, JsonValue.Create(stringValue));
                            break;
                    }
                }
            }

            if (!itemId.HasValue)
            {
                data.Data.Add(tableItem!);
            }

            contentNode.Config = JsonSerializer.SerializeToDocument(data);
            await dbContext.SaveChangesAsync();

            Response.Headers.Append("HX-Trigger", "refreshWorkspace, toggleEditor");
        }

        var viewModel = new MacInterfaceTableItemEditorPanelViewModel
        {
            WorkspaceId = contentNode.Workspace.PublicId,
            InterfaceId = contentNode.DocId,
            Structure = data.Structure,
            Content = tableItem
        };

        return PartialView("~/Views/MacTable/Partials/TableItemEditorPanel.cshtml", viewModel);
    }

    [HttpDelete("workspace/{interfaceId}/item/{itemId:guid}")]
    public async Task<IActionResult> DeleteTableItem(string interfaceId, Guid itemId)
    {
        if (!GuidShortId.TryParse(interfaceId, out var interfaceGuid))
        {
            return NotFound();
        }

        var contentNode = await dbContext.GooeyInterfaces
            .Include(x => x.Workspace)
            .FirstAsync(x => x.DocId.Equals(interfaceGuid));

        var data = contentNode.Config.Deserialize<MacTableJsonDataModel>();

        data.Data = data.Data
            .Where(x => !x.Identifier.Equals(itemId))
            .ToList();

        contentNode.Config = JsonSerializer.SerializeToDocument(data);
        await dbContext.SaveChangesAsync();

        Response.Headers.Append("HX-Trigger", "refreshWorkspace");

        return Ok();

        // var viewModel = await WorkspaceViewModelForInterfaceId(interfaceId);
        //
        // return PartialView("~/Views/MacTable/Workspace.cshtml", viewModel);
    }

    private static void UpdateKeyOrAddIfNotExists(Dictionary<string, object>? json, string key, JsonNode? value)
    {
        if (json is null || value is null)
            return;

        if (json.ContainsKey(key))
        {
            json[key] = value;
            return;
        }

        json.Add(key, value);
    }
}
