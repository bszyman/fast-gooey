using System.Text.Json;
using FastGooey.Attributes;
using FastGooey.Database;
using FastGooey.Models;
using FastGooey.Models.JsonDataModels;
using FastGooey.Models.ViewModels.RssFeed;
using FastGooey.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FastGooey.Controllers.Interfaces;

[Authorize]
[AuthorizeWorkspaceAccess]
[Route("Workspaces/{workspaceId:guid}/Interfaces/AppleMobile/Form")]
public class AppleMobileFormController(
    ILogger<AppleMobileFormController> logger, 
    IKeyValueService keyValueService,
    ApplicationDbContext dbContext): 
    BaseStudioController(keyValueService, dbContext)
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    
    [HttpGet("workspace")]
    public IActionResult Workspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/Form.cshtml");
    }
    
    // [HttpPost("create-widget")]
    // public async Task<IActionResult> CreateWidget()
    // {
    //     var workspace = GetWorkspace();
    //     var data = new AppleMobileListJsonDataModel();
    //     
    //     var contentNode = new GooeyInterface
    //     {
    //         WorkspaceId = workspace.Id,
    //         Workspace = workspace,
    //         Platform = "AppleMobile",
    //         ViewType = "Form",
    //         Name = "New List Interface",
    //         Config = JsonSerializer.SerializeToDocument(data)
    //     };
    //
    //     await dbContext.GooeyInterfaces.AddAsync(contentNode);
    //     await dbContext.SaveChangesAsync();
    //
    //     var workspaceViewModel = await WorkspaceViewModelForInterfaceId(contentNode.DocId);
    //     var viewModel = new RssViewModel
    //     {
    //         WorkspaceViewModel = workspaceViewModel
    //     };
    //     
    //     Response.Headers.Append("HX-Trigger", "refreshNavigation");
    //     
    //     return PartialView("~/Views/RssFeed/Index.cshtml", viewModel);
    // }
    
    [HttpGet("entries-workspace")]
    public IActionResult FormEntriesWorkspace()
    {
        return PartialView("~/Views/AppleMobile/Workspaces/FormEntriesWorkspace.cshtml");
    }
    
    [HttpGet("FormFieldSelectorPanel")]
    public IActionResult FormFieldSelectorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldSelectorPanel.cshtml");
    }
    
    [HttpGet("FormSubmissionViewerPanel")]
    public IActionResult FormSubmissionViewerPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormSubmissionViewerPanel.cshtml");
    }
    
    [HttpGet("FormFieldTextEditorPanel")]
    public IActionResult FormFieldTextEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldLongTextEditorPanel")]
    public IActionResult FormFieldLongTextEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldLongTextEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldCheckboxEditorPanel")]
    public IActionResult FormFieldCheckboxEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldCheckboxEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDateEditorPanel")]
    public IActionResult FormFieldDateEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldDateEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldTimeEditorPanel")]
    public IActionResult FormFieldTimeEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldTimeEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldDropDownEditorPanel")]
    public IActionResult FormFieldDropDownEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldDropDownEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldMultiSelectEditorPanel")]
    public IActionResult FormFieldMultiSelectEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldMultiSelectEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldFileEditorPanel")]
    public IActionResult FormFieldFileEditorPanel()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldFileEditorPanel.cshtml");
    }
    
    [HttpGet("FormFieldBlankOption")]
    public IActionResult FormFieldBlankOption()
    {
        return PartialView("~/Views/AppleMobile/Partials/Forms/FormFieldBlankOption.cshtml");
    }
}